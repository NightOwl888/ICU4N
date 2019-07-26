using ICU4N.Impl;
using ICU4N.Globalization;
using System;
using System.Globalization;

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
        //[Obsolete("This API is ICU internal only.")]
        private int internalOptions; // ICU4N specific - made internalOptions private, since this is not intended for use by subclasses

        private CaseMap(int opt) { internalOptions = opt; } // ICU4N TODO: API - see whether it makes sense to make a [Flags] enum for opt

        private static int GetCaseLocale(CultureInfo locale)
        {
            if (locale == null)
            {
                locale = CultureInfo.CurrentCulture;
            }
            return UCaseProps.GetCaseLocale(locale);
        }

        /// <summary>
        /// Returns lowercasing object with default options.
        /// </summary>
        /// <returns>Lowercasing object with default options.</returns>
        /// <draft>ICU 59</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static Lower ToLower() { return Lower.DEFAULT; }
        /// <summary>
        /// Returns uppercasing object with default options.
        /// </summary>
        /// <returns>Uppercasing object with default options.</returns>
        /// <draft>ICU 59</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static Upper ToUpper() { return Upper.DEFAULT; }
        /// <summary>
        /// Returns titlecasing object with default options.
        /// </summary>
        /// <returns>Titlecasing object with default options.</returns>
        /// <draft>ICU 59</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static Title ToTitle() { return Title.DEFAULT; }
        /// <summary>
        /// Returns case folding object with default options.
        /// </summary>
        /// <returns>Case folding object with default options.</returns>
        /// <draft>ICU 59</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static Fold ToFold() { return Fold.DEFAULT; } // ICU4N specific - renamed from Fold() because of naming collision

        // ICU4N specific - removed OmitUnchangedText() from abstract class, since we need it
        // to return a different type in each subclass and making this class generic would complicate things
        // considerably.

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

            /// <summary>
            /// Returns an instance that behaves like this one but
            /// omits unchanged text when case-mapping with <see cref="Edits"/>.
            /// </summary>
            /// <returns>An options object with this option.</returns>
            /// <draft>ICU 59</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            public Lower OmitUnchangedText()
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

            /// <summary>
            /// Returns an instance that behaves like this one but
            /// omits unchanged text when case-mapping with <see cref="Edits"/>.
            /// </summary>
            /// <returns>An options object with this option.</returns>
            /// <draft>ICU 59</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            public Upper OmitUnchangedText()
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

            /// <summary>
            /// Returns an instance that behaves like this one but
            /// titlecases the string as a whole rather than each word.
            /// (Titlecases only the character at index 0, possibly adjusted.)
            /// <para/>
            /// It is an error to specify multiple titlecasing iterator options together,
            /// including both an option and an explicit <see cref="BreakIterator"/>.
            /// </summary>
            /// <returns>An options object with this option.</returns>
            /// <seealso cref="AdjustToCased()"/>
            /// <draft>ICU 60</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            public Title WholeString()
            {
                return new Title(CaseMapImpl.AddTitleIteratorOption(
                        internalOptions, CaseMapImpl.TITLECASE_WHOLE_STRING));
            }

            /// <summary>
            /// Returns an instance that behaves like this one but
            /// titlecases sentences rather than words.
            /// (Titlecases only the first character of each sentence, possibly adjusted.)
            /// <para/>
            /// It is an error to specify multiple titlecasing iterator options together,
            /// including both an option and an explicit <see cref="BreakIterator"/>.
            /// </summary>
            /// <returns>An options object with this option.</returns>
            /// <seealso cref="AdjustToCased()"/>
            /// <draft>ICU 60</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            public Title Sentences()
            {
                return new Title(CaseMapImpl.AddTitleIteratorOption(
                        internalOptions, CaseMapImpl.TITLECASE_SENTENCES));
            }

            /// <summary>
            /// Returns an instance that behaves like this one but
            /// omits unchanged text when case-mapping with <see cref="Edits"/>.
            /// </summary>
            /// <returns>An options object with this option.</returns>
            /// <draft>ICU 59</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            public Title OmitUnchangedText()
            {
                if (internalOptions == 0 || internalOptions == CaseMapImpl.OMIT_UNCHANGED_TEXT)
                {
                    return OMIT_UNCHANGED;
                }
                return new Title(internalOptions | CaseMapImpl.OMIT_UNCHANGED_TEXT);
            }

            /// <summary>
            /// Returns an instance that behaves like this one but
            /// does not lowercase non-initial parts of words when titlecasing.
            /// <para/>
            /// By default, titlecasing will titlecase the character at each
            /// (possibly adjusted) <see cref="BreakIterator"/> index and
            /// lowercase all other characters up to the next iterator index.
            /// With this option, the other characters will not be modified.
            /// </summary>
            /// <returns>An options object with this option.</returns>
            /// <seealso cref="UCharacter.TitleCaseNoLowerCase"/>
            /// <seealso cref="AdjustToCased()"/>
            /// <draft>ICU 59</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            public Title NoLowercase()
            {
                return new Title(internalOptions | UCharacter.TitleCaseNoLowerCase);
            }

            /// <summary>
            /// Returns an instance that behaves like this one but
            /// does not adjust the titlecasing <see cref="BreakIterator"/> indexes;
            /// titlecases exactly the characters at breaks from the iterator.
            /// <para/>
            /// By default, titlecasing will take each break iterator index,
            /// adjust it to the next relevant character (see <see cref="AdjustToCased()"/>),
            /// and titlecase that one.
            /// <para/>
            /// Other characters are lowercased.
            /// </summary>
            /// <returns>An options object with this option.</returns>
            /// <seealso cref="UCharacter.TitleCaseNoBreakAdjustment"/>
            /// <draft>ICU 59</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            public Title NoBreakAdjustment()
            {
                return new Title(CaseMapImpl.AddTitleAdjustmentOption(
                        internalOptions, UCharacter.TitleCaseNoBreakAdjustment));
            }

            /// <summary>
            /// Returns an instance that behaves like this one but
            /// adjusts each titlecasing <see cref="BreakIterator"/> index to the next cased character.
            /// (See the Unicode Standard, chapter 3, Default Case Conversion, R3 toTitlecase(X).)
            /// </summary>
            /// <remarks>
            /// This used to be the default index adjustment in ICU.
            /// Since ICU 60, the default index adjustment is to the next character that is
            /// a letter, number, symbol, or private use code point.
            /// (Uncased modifier letters are skipped.)
            /// The difference in behavior is small for word titlecasing,
            /// but the new adjustment is much better for whole-string and sentence titlecasing:
            /// It yields "49ers" and "«丰(abc)»" instead of "49Ers" and "«丰(Abc)»".
            /// <para/>
            /// It is an error to specify multiple titlecasing adjustment options together.
            /// </remarks>
            /// <returns>An options object with this option.</returns>
            /// <draft>ICU 60</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            /// <seealso cref="NoBreakAdjustment()"/>
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
            private static readonly Fold TURKIC = new Fold(UCharacter.FoldCaseExcludeSpecialI);
            private static readonly Fold OMIT_UNCHANGED = new Fold(CaseMapImpl.OMIT_UNCHANGED_TEXT);
            private static readonly Fold TURKIC_OMIT_UNCHANGED = new Fold(
                    UCharacter.FoldCaseExcludeSpecialI | CaseMapImpl.OMIT_UNCHANGED_TEXT);
            internal Fold(int opt)
                        : base(opt)
            {
            }

            /// <summary>
            /// Returns an instance that behaves like this one but
            /// omits unchanged text when case-mapping with <see cref="Edits"/>.
            /// </summary>
            /// <returns>An options object with this option.</returns>
            /// <draft>ICU 59</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            public Fold OmitUnchangedText()
            {
                return (internalOptions & UCharacter.FoldCaseExcludeSpecialI) == 0 ?
                        OMIT_UNCHANGED : TURKIC_OMIT_UNCHANGED;
            }

            /// <summary>
            /// Returns an instance that behaves like this one but
            /// handles dotted I and dotless i appropriately for Turkic languages (tr, az).
            /// <para/>
            /// Uses the Unicode CaseFolding.txt mappings marked with 'T' that
            /// are to be excluded for default mappings and
            /// included for the Turkic-specific mappings.
            /// </summary>
            /// <returns>An options object with this option.</returns>
            /// <seealso cref="UCharacter.FoldCaseExcludeSpecialI"/>
            /// <draft>ICU 59</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
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
