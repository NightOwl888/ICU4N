using ICU4N.Impl;
using ICU4N.Globalization;
using System;
using System.Globalization;
using J2N.Text;
using System.Text;
using ICU4N.Support.Text;
#nullable enable

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
    public abstract partial class CaseMap // ICU4N TODO: API - Evaluate whether it makes sense to have these as fluent. These are low level ops that would best be made static, although currently CaseMapImpl fills that role. FoldCase() should be a set of extension methods for ReadOnlySpan<char>, Span<char>, and string, but the others will collide with .NET extenions. We should also have high-level culture aware methods on a UTextInfo class that is provided by UCultureInfo.TextInfo. It would be simplest to put all of these options in one place there.
    {
        //[Obsolete("This API is ICU internal only.")]
        internal int internalOptions; // ICU4N specific - made internalOptions internal, since this is not intended for use by subclasses

        internal CaseMap(int opt) { internalOptions = opt; } // ICU4N TODO: API - see whether it makes sense to make a [Flags] enum for opt

        internal static CaseLocale GetCaseLocale(CultureInfo? locale)
        {
            if (locale == null)
            {
                locale = CultureInfo.CurrentCulture;
            }
            return UCaseProperties.GetCaseLocale(locale);
        }

        /// <summary>
        /// Returns lowercasing object with default options.
        /// </summary>
        /// <returns>Lowercasing object with default options.</returns>
        /// <draft>ICU 59</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static LowerCaseMap ToLower() => LowerCaseMap.DEFAULT;
        /// <summary>
        /// Returns uppercasing object with default options.
        /// </summary>
        /// <returns>Uppercasing object with default options.</returns>
        /// <draft>ICU 59</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static UpperCaseMap ToUpper() => UpperCaseMap.DEFAULT;
        /// <summary>
        /// Returns titlecasing object with default options.
        /// </summary>
        /// <returns>Titlecasing object with default options.</returns>
        /// <draft>ICU 59</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static TitleCaseMap ToTitle() => TitleCaseMap.DEFAULT;
        /// <summary>
        /// Returns case folding object with default options.
        /// </summary>
        /// <returns>Case folding object with default options.</returns>
        /// <draft>ICU 59</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static FoldCaseMap ToFold() => FoldCaseMap.DEFAULT; // ICU4N specific - renamed from Fold() because of naming collision

        // ICU4N specific - removed OmitUnchangedText() from abstract class, since we need it
        // to return a different type in each subclass and making this class generic would complicate things
        // considerably.

        // ICU4N specific - de-nested Lower and renamed LowerCaseMap

        // ICU4N specific - de-nested Upper and renamed UpperCaseMap

        // ICU4N specific - de-nested Title and renamed TitleCaseMap

        // ICU4N specific - de-nested Fold and renamed FoldCaseMap
    }

    /// <summary>
    /// Lowercasing options and methods. Immutable.
    /// </summary>
    /// <seealso cref="CaseMap.ToLower()"/>
    /// <draft>ICU 59</draft>
    /// <provisional>This API might change or be removed in a future release.</provisional>
    public sealed partial class LowerCaseMap : CaseMap
    {
        internal static readonly LowerCaseMap DEFAULT = new LowerCaseMap(0);
        private static readonly LowerCaseMap OMIT_UNCHANGED = new LowerCaseMap(CaseMapImpl.OmitUnchangedText);
        internal LowerCaseMap(int opt)
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
        public LowerCaseMap OmitUnchangedText() => OMIT_UNCHANGED;

        // ICU4N specific - Apply(CultureInfo locale, ICharSequence src) moved to CaseMap.generated.tt

        // ICU4N specific - Apply(
        //    CultureInfo locale, string src, StringBuilder dest, Edits edits) moved to CaseMap.generated.tt
    }

    /// <summary>
    /// Uppercasing options and methods. Immutable.
    /// </summary>
    /// <seealso cref="CaseMap.ToUpper()"/>
    /// <draft>ICU 59</draft>
    /// <provisional>This API might change or be removed in a future release.</provisional>
    public sealed partial class UpperCaseMap : CaseMap
    {
        internal static readonly UpperCaseMap DEFAULT = new UpperCaseMap(0);
        private static readonly UpperCaseMap OMIT_UNCHANGED = new UpperCaseMap(CaseMapImpl.OmitUnchangedText);
        internal UpperCaseMap(int opt)
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
        public UpperCaseMap OmitUnchangedText() => OMIT_UNCHANGED;

        // ICU4N specific - Apply(CultureInfo locale, ICharSequence src) moved to CaseMap.generated.tt

        // ICU4N specific - Apply<T>(
        //    CultureInfo locale, string src, T dest, Edits edits) where T : IAppendable moved to CaseMap.generated.tt
    }

    /// <summary>
    /// Titlecasing options and methods. Immutable.
    /// </summary>
    /// <seealso cref="CaseMap.ToTitle()"/>
    /// <draft>ICU 59</draft>
    /// <provisional>This API might change or be removed in a future release.</provisional>
    public sealed partial class TitleCaseMap : CaseMap
    {
        internal static readonly TitleCaseMap DEFAULT = new TitleCaseMap(0);
        private static readonly TitleCaseMap OMIT_UNCHANGED = new TitleCaseMap(CaseMapImpl.OmitUnchangedText);
        private TitleCaseMap(int opt)
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
        public TitleCaseMap WholeString()
        {
            return new TitleCaseMap(CaseMapImpl.AddTitleIteratorOption(
                internalOptions, CaseMapImpl.TitleCaseWholeString));
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
        public TitleCaseMap Sentences()
        {
            return new TitleCaseMap(CaseMapImpl.AddTitleIteratorOption(
                internalOptions, CaseMapImpl.TitleCaseSentences));
        }

        /// <summary>
        /// Returns an instance that behaves like this one but
        /// omits unchanged text when case-mapping with <see cref="Edits"/>.
        /// </summary>
        /// <returns>An options object with this option.</returns>
        /// <draft>ICU 59</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public TitleCaseMap OmitUnchangedText()
        {
            if (internalOptions == 0 || internalOptions == CaseMapImpl.OmitUnchangedText)
            {
                return OMIT_UNCHANGED;
            }
            return new TitleCaseMap(internalOptions | CaseMapImpl.OmitUnchangedText);
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
        /// <seealso cref="UChar.TitleCaseNoLowerCase"/>
        /// <seealso cref="AdjustToCased()"/>
        /// <draft>ICU 59</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public TitleCaseMap NoLowercase()
        {
            return new TitleCaseMap(internalOptions | UChar.TitleCaseNoLowerCase);
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
        /// <seealso cref="UChar.TitleCaseNoBreakAdjustment"/>
        /// <draft>ICU 59</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public TitleCaseMap NoBreakAdjustment()
        {
            return new TitleCaseMap(CaseMapImpl.AddTitleAdjustmentOption(
                internalOptions, UChar.TitleCaseNoBreakAdjustment));
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
        public TitleCaseMap AdjustToCased()
        {
            return new TitleCaseMap(CaseMapImpl.AddTitleAdjustmentOption(
                internalOptions, CaseMapImpl.TitleCaseAdjustToCased));
        }



        // ICU4N specific - Apply(CultureInfo locale, BreakIterator iter, ICharSequence src) moved to CaseMap.generated.tt

        // ICU4N specific - Apply<T>(
        //    CultureInfo locale, BreakIterator iter, ICharSequence src, T dest, Edits edits) where dest : IAppendable moved to CaseMap.generated.tt

        // ICU4N specific overload for convenience
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
        /// <param name="iter">
        /// A break iterator to find the first characters of words that are to be titlecased.
        /// It is set to the source string (SetText())
        /// and used one or more times for iteration (First() and Next()).
        /// If null, then a word break iterator for the locale is used
        /// (or something equivalent).
        /// </param>
        /// <param name="src">The original string.</param>
        /// <returns>The result string.</returns>
        /// <seealso cref="UChar.ToTitleCase(CultureInfo, string, BreakIterator, int)"/>
        /// <draft>ICU 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public string Apply(BreakIterator? iter, ReadOnlyMemory<char> src)
        {
            CultureInfo locale = CultureInfo.CurrentCulture;
            iter = CaseMapImpl.GetTitleBreakIterator(locale, internalOptions, iter);
            iter.SetText(src);
            return CaseMapImpl.ToTitle(GetCaseLocale(locale), internalOptions, iter, src.Span);
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
        /// <seealso cref="UChar.ToTitleCase(CultureInfo, string, BreakIterator, int)"/>
        /// <draft>ICU 60</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public string Apply(CultureInfo? locale, BreakIterator? iter, ReadOnlyMemory<char> src)
        {
            if (iter == null && locale == null)
            {
                locale = CultureInfo.CurrentCulture;
            }
            iter = CaseMapImpl.GetTitleBreakIterator(locale!, internalOptions, iter);
            iter.SetText(src);
            return CaseMapImpl.ToTitle(GetCaseLocale(locale), internalOptions, iter, src.Span);
        }

        // ICU4N specific overload for convenience
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
        /// This function calls <see cref="Edits.Reset()"/> first. <paramref name="edits"/> can be null.
        /// </param>
        /// <returns><paramref name="dest"/> with the result string (or only changes) appended.</returns>
        /// <seealso cref="UChar.ToTitleCase(CultureInfo, string, BreakIterator, int)"/>
        /// <draft>ICU 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public StringBuilder Apply(
            BreakIterator? iter, ReadOnlyMemory<char> src, StringBuilder dest, Edits? edits)
        {
            CultureInfo locale = CultureInfo.CurrentCulture;
            iter = CaseMapImpl.GetTitleBreakIterator(locale, internalOptions, iter);
            iter.SetText(src);
            return CaseMapImpl.ToTitle(
                    GetCaseLocale(locale), internalOptions, iter, src.Span, dest, edits);
        }


        // ICU4N specific overload for convenience
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
        /// This function calls <see cref="Edits.Reset()"/> first. <paramref name="edits"/> can be null.
        /// </param>
        /// <returns><paramref name="dest"/> with the result string (or only changes) appended.</returns>
        /// <seealso cref="UChar.ToTitleCase(CultureInfo, string, BreakIterator, int)"/>
        /// <draft>ICU 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public T Apply<T>(
            BreakIterator? iter, ReadOnlyMemory<char> src, T dest, Edits? edits) where T : IAppendable
        {
            CultureInfo locale = CultureInfo.CurrentCulture;
            iter = CaseMapImpl.GetTitleBreakIterator(locale, internalOptions, iter);
            iter.SetText(src);
            return CaseMapImpl.ToTitle(
                    GetCaseLocale(locale), internalOptions, iter, src.Span, dest, edits);
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
        /// This function calls <see cref="Edits.Reset()"/> first. <paramref name="edits"/> can be null.
        /// </param>
        /// <returns><paramref name="dest"/> with the result string (or only changes) appended.</returns>
        /// <seealso cref="UChar.ToTitleCase(CultureInfo, string, BreakIterator, int)"/>
        /// <draft>ICU 60</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public StringBuilder Apply(
            CultureInfo? locale, BreakIterator? iter, ReadOnlyMemory<char> src, StringBuilder dest, Edits? edits)
        {
            if (iter == null && locale == null)
            {
                locale = CultureInfo.CurrentCulture;
            }
            iter = CaseMapImpl.GetTitleBreakIterator(locale!, internalOptions, iter);
            iter.SetText(src);
            return CaseMapImpl.ToTitle(
                    GetCaseLocale(locale), internalOptions, iter, src.Span, dest, edits);
        }

        // ICU4N: Avoid using StringBuilder internally
        internal void Apply(
            CultureInfo? locale, BreakIterator? iter, ReadOnlyMemory<char> src, ref ValueStringBuilder dest, Edits? edits)
        {
            if (iter == null && locale == null)
            {
                locale = CultureInfo.CurrentCulture;
            }
            iter = CaseMapImpl.GetTitleBreakIterator(locale!, internalOptions, iter);
            iter.SetText(src);
            CaseMapImpl.ToTitle(
                    GetCaseLocale(locale), internalOptions, iter, src.Span, ref dest, edits);
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
        /// This function calls <see cref="Edits.Reset()"/> first. <paramref name="edits"/> can be null.
        /// </param>
        /// <returns><paramref name="dest"/> with the result string (or only changes) appended.</returns>
        /// <seealso cref="UChar.ToTitleCase(CultureInfo, string, BreakIterator, int)"/>
        /// <draft>ICU 60</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public T Apply<T>(
            CultureInfo? locale, BreakIterator? iter, ReadOnlyMemory<char> src, T dest, Edits? edits) where T : IAppendable
        {
            if (iter == null && locale == null)
            {
                locale = CultureInfo.CurrentCulture;
            }
            iter = CaseMapImpl.GetTitleBreakIterator(locale!, internalOptions, iter);
            iter.SetText(src);
            return CaseMapImpl.ToTitle(
                    GetCaseLocale(locale), internalOptions, iter, src.Span, dest, edits);
        }
    }

    /// <summary>
    /// Case folding options and methods. Immutable.
    /// </summary>
    /// <seealso cref="CaseMap.ToFold()"/>
    /// <draft>ICU 59</draft>
    /// <provisional>This API might change or be removed in a future release.</provisional>
    public sealed partial class FoldCaseMap : CaseMap
    {
        internal static readonly FoldCaseMap DEFAULT = new FoldCaseMap(0);
        private static readonly FoldCaseMap TURKIC = new FoldCaseMap(UChar.FoldCaseExcludeSpecialI);
        private static readonly FoldCaseMap OMIT_UNCHANGED = new FoldCaseMap(CaseMapImpl.OmitUnchangedText);
        private static readonly FoldCaseMap TURKIC_OMIT_UNCHANGED = new FoldCaseMap(
                UChar.FoldCaseExcludeSpecialI | CaseMapImpl.OmitUnchangedText);
        internal FoldCaseMap(int opt)
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
        public FoldCaseMap OmitUnchangedText()
        {
            return (internalOptions & UChar.FoldCaseExcludeSpecialI) == 0 ?
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
        /// <seealso cref="UChar.FoldCaseExcludeSpecialI"/>
        /// <draft>ICU 59</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public FoldCaseMap Turkic()
        {
            return (internalOptions & CaseMapImpl.OmitUnchangedText) == 0 ?
                TURKIC : TURKIC_OMIT_UNCHANGED;
        }

        // ICU4N specific - Apply(ICharSequence src) moved to CaseMap.generated.tt

        // ICU4N specific - Apply(
        //    string src, StringBuilder dest, Edits edits) moved to CaseMap.generated.tt
    }
}
