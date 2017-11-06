using ICU4N.Lang;
using ICU4N.Support;
using ICU4N.Support.Text;
using ICU4N.Text;
using ICU4N.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace ICU4N.Impl
{
    public sealed class CaseMapImpl
    {
        /**
     * Implementation of UCaseProps.ContextIterator, iterates over a String.
     * See ustrcase.c/utf16_caseContextIterator().
     */
        public sealed class StringContextIterator : UCaseProps.IContextIterator
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="src">String to iterate over.</param>
            public StringContextIterator(string src)
                : this(src.ToCharSequence())
            {
            }

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="src">String to iterate over.</param>
            public StringContextIterator(StringBuilder src)
                : this(src.ToCharSequence())
            {
            }

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="src">String to iterate over.</param>
            public StringContextIterator(char[] src)
                : this(src.ToCharSequence())
            {
            }

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="src">String to iterate over.</param>
            internal StringContextIterator(ICharSequence src)
            {
                this.s = src;
                limit = src.Length;
                cpStart = cpLimit = index = 0;
                dir = 0;
            }

            /**
             * Set the iteration limit for nextCaseMapCP() to an index within the string.
             * If the limit parameter is negative or past the string, then the
             * string length is restored as the iteration limit.
             *
             * <p>This limit does not affect the next() function which always
             * iterates to the very end of the string.
             *
             * @param lim The iteration limit.
             */
            public void SetLimit(int lim)
            {
                if (0 <= lim && lim <= s.Length)
                {
                    limit = lim;
                }
                else
                {
                    limit = s.Length;
                }
            }

            /**
             * Move to the iteration limit without fetching code points up to there.
             */
            public void MoveToLimit()
            {
                cpStart = cpLimit = limit;
            }

            /**
             * Iterate forward through the string to fetch the next code point
             * to be case-mapped, and set the context indexes for it.
             *
             * <p>When the iteration limit is reached (and -1 is returned),
             * getCPStart() will be at the iteration limit.
             *
             * <p>Iteration with next() does not affect the position for nextCaseMapCP().
             *
             * @return The next code point to be case-mapped, or <0 when the iteration is done.
             */
            public int NextCaseMapCP()
            {
                cpStart = cpLimit;
                if (cpLimit < limit)
                {
                    int c = Character.CodePointAt(s, cpLimit);
                    cpLimit += Character.CharCount(c);
                    return c;
                }
                else
                {
                    return -1;
                }
            }

            /**
             * Returns the start of the code point that was last returned
             * by nextCaseMapCP().
             */
            public int CPStart
            {
                get { return cpStart; }
            }

            /**
             * Returns the limit of the code point that was last returned
             * by nextCaseMapCP().
             */
            public int CPLimit
            {
                get { return cpLimit; }
            }

            public int CPLength
            {
                get { return cpLimit - cpStart; }
            }

            // implement UCaseProps.ContextIterator
            // The following code is not used anywhere in this private class
            public void Reset(int direction)
            {
                if (direction > 0)
                {
                    /* reset for forward iteration */
                    dir = 1;
                    index = cpLimit;
                }
                else if (direction < 0)
                {
                    /* reset for backward iteration */
                    dir = -1;
                    index = cpStart;
                }
                else
                {
                    // not a valid direction
                    dir = 0;
                    index = 0;
                }
            }

            public int Next()
            {
                int c;

                if (dir > 0 && index < s.Length)
                {
                    c = Character.CodePointAt(s, index);
                    index += Character.CharCount(c);
                    return c;
                }
                else if (dir < 0 && index > 0)
                {
                    c = Character.CodePointBefore(s, index);
                    index -= Character.CharCount(c);
                    return c;
                }
                return -1;
            }

            // variables
            private ICharSequence s;
            private int index, limit, cpStart, cpLimit;
            private int dir; // 0=initial state  >0=forward  <0=backward
        }

        public const int TITLECASE_WHOLE_STRING = 0x20;
        public const int TITLECASE_SENTENCES = 0x40;

        /**
         * Bit mask for the titlecasing iterator options bit field.
         * Currently only 3 out of 8 values are used:
         * 0 (words), TITLECASE_WHOLE_STRING, TITLECASE_SENTENCES.
         * See stringoptions.h.
         * @internal
         */
        private const int TITLECASE_ITERATOR_MASK = 0xe0;

        public const int TITLECASE_ADJUST_TO_CASED = 0x400;

        /**
         * Bit mask for the titlecasing index adjustment options bit set.
         * Currently two bits are defined:
         * TITLECASE_NO_BREAK_ADJUSTMENT, TITLECASE_ADJUST_TO_CASED.
         * See stringoptions.h.
         * @internal
         */
        private const int TITLECASE_ADJUSTMENT_MASK = 0x600;

        public static int AddTitleAdjustmentOption(int options, int newOption)
        {
            int adjOptions = options & TITLECASE_ADJUSTMENT_MASK;
            if (adjOptions != 0 && adjOptions != newOption)
            {
                throw new ArgumentException("multiple titlecasing index adjustment options");
            }
            return options | newOption;
        }

        private static readonly int LNS =
                (1 << (int)UnicodeCategory.UppercaseLetter) |
                (1 << (int)UnicodeCategory.LowercaseLetter) |
                (1 << (int)UnicodeCategory.TitlecaseLetter) |
                // Not MODIFIER_LETTER: We count only cased modifier letters.
                (1 << (int)UnicodeCategory.OtherLetter) |

                (1 << (int)UnicodeCategory.DecimalDigitNumber) |
                (1 << (int)UnicodeCategory.LetterNumber) |
                (1 << (int)UnicodeCategory.OtherNumber) |

                (1 << (int)UnicodeCategory.MathSymbol) |
                (1 << (int)UnicodeCategory.CurrencySymbol) |
                (1 << (int)UnicodeCategory.ModifierSymbol) |
                (1 << (int)UnicodeCategory.OtherSymbol) |

                (1 << (int)UnicodeCategory.PrivateUse);

        private static bool IsLNS(int c)
        {
            // Letter, number, symbol,
            // or a private use code point because those are typically used as letters or numbers.
            // Consider modifier letters only if they are cased.
            int gc = UCharacterProperty.INSTANCE.GetType(c);
            return ((1 << gc) & LNS) != 0 ||
                    (gc == (int)UnicodeCategory.ModifierLetter &&
                        UCaseProps.INSTANCE.GetType(c) != UCaseProps.NONE);
        }

        public static int AddTitleIteratorOption(int options, int newOption)
        {
            int iterOptions = options & TITLECASE_ITERATOR_MASK;
            if (iterOptions != 0 && iterOptions != newOption)
            {
                throw new ArgumentException("multiple titlecasing iterator options");
            }
            return options | newOption;
        }

        public static BreakIterator GetTitleBreakIterator(
                CultureInfo locale, int options, BreakIterator iter)
        {
            options &= TITLECASE_ITERATOR_MASK;
            if (options != 0 && iter != null)
            {
                throw new ArgumentException(
                        "titlecasing iterator option together with an explicit iterator");
            }
            if (iter == null)
            {
                switch (options)
                {
                    case 0:
                        iter = BreakIterator.GetWordInstance(locale);
                        break;
                    case TITLECASE_WHOLE_STRING:
                        iter = new WholeStringBreakIterator();
                        break;
                    case TITLECASE_SENTENCES:
                        iter = BreakIterator.GetSentenceInstance(locale);
                        break;
                    default:
                        throw new ArgumentException("unknown titlecasing iterator option");
                }
            }
            return iter;
        }

        public static BreakIterator GetTitleBreakIterator(
                ULocale locale, int options, BreakIterator iter)
        {
            options &= TITLECASE_ITERATOR_MASK;
            if (options != 0 && iter != null)
            {
                throw new ArgumentException(
                        "titlecasing iterator option together with an explicit iterator");
            }
            if (iter == null)
            {
                switch (options)
                {
                    case 0:
                        iter = BreakIterator.GetWordInstance(locale);
                        break;
                    case TITLECASE_WHOLE_STRING:
                        iter = new WholeStringBreakIterator();
                        break;
                    case TITLECASE_SENTENCES:
                        iter = BreakIterator.GetSentenceInstance(locale);
                        break;
                    default:
                        throw new ArgumentException("unknown titlecasing iterator option");
                }
            }
            return iter;
        }

        /**
         * Omit unchanged text when case-mapping with Edits.
         */
        public static readonly int OMIT_UNCHANGED_TEXT = 0x4000;

        private sealed class WholeStringBreakIterator : BreakIterator
        {
            private int length;

            private static void NotImplemented()
            {
                throw new NotSupportedException("should not occur");
            }

            public override int First()
            {
                return 0;
            }

            public override int Last()
            {
                NotImplemented();
                return 0;
            }

            public override int Next(int n)
            {
                NotImplemented();
                return 0;
            }

            public override int Next()
            {
                return length;
            }

            public override int Previous()
            {
                NotImplemented();
                return 0;
            }

            public override int Following(int offset)
            {
                NotImplemented();
                return 0;
            }

            public override int Current
            {
                get
                {
                    NotImplemented();
                    return 0;
                }
            }

            public override CharacterIterator GetText()
            {
                NotImplemented();
                return null;
            }

            public override void SetText(CharacterIterator newText)
            {
                length = newText.EndIndex;
            }

            internal override void SetText(ICharSequence newText)
            {
                length = newText.Length;
            }

            public override void SetText(string newText)
            {
                length = newText.Length;
            }
        }

        private static int AppendCodePoint(StringBuilder a, int c)
        {
            if (c <= char.MaxValue)
            {
                a.Append((char)c);
                return 1;
            }
            else
            {
                a.Append((char)(0xd7c0 + (c >> 10)));
                a.Append((char)(Character.MIN_LOW_SURROGATE + (c & 0x3ff)));
                return 2;
            }
        }

        /**
         * Appends a full case mapping result, see {@link UCaseProps#MAX_STRING_LENGTH}.
         * @throws IOException
         */
        private static void AppendResult(int result, StringBuilder dest,
                int cpLength, int options, Edits edits)
        {
            // Decode the result.
            if (result < 0)
            {
                // (not) original code point
                if (edits != null)
                {
                    edits.AddUnchanged(cpLength);
                }
                if ((options & OMIT_UNCHANGED_TEXT) != 0)
                {
                    return;
                }
                AppendCodePoint(dest, ~result);
            }
            else if (result <= UCaseProps.MAX_STRING_LENGTH)
            {
                // The mapping has already been appended to result.
                if (edits != null)
                {
                    edits.AddReplace(cpLength, result);
                }
            }
            else
            {
                // Append the single-code point mapping.
                int length = AppendCodePoint(dest, result);
                if (edits != null)
                {
                    edits.AddReplace(cpLength, length);
                }
            }
        }

        private static void AppendUnchanged(ICharSequence src, int start, int length,
                StringBuilder dest, int options, Edits edits)
        {
            if (length > 0)
            {
                if (edits != null)
                {
                    edits.AddUnchanged(length);
                }
                if ((options & OMIT_UNCHANGED_TEXT) != 0)
                {
                    return;
                }
                dest.Append(src, start, start + length);
            }
        }

        private static string ApplyEdits(ICharSequence src, StringBuilder replacementChars, Edits edits)
        {
            if (!edits.HasChanges)
            {
                return src.ToString();
            }
            StringBuilder result = new StringBuilder(src.Length + edits.LengthDelta);
            for (Edits.Iterator ei = edits.GetCoarseIterator(); ei.Next();)
            {
                if (ei.HasChange)
                {
                    int i = ei.ReplacementIndex;
                    result.Append(replacementChars.ToString(), i, ei.NewLength); // ICU4N: (i + ei.NewLength) - i == ei.NewLength
                }
                else
                {
                    int i = ei.SourceIndex;
                    result.Append(src, i, i + ei.OldLength); // ICU4N: Extension method takes care of end to length conversion
                }
            }
            return result.ToString();
        }

        private static void InternalToLower(int caseLocale, int options, StringContextIterator iter,
                StringBuilder dest, Edits edits)
        {
            int c;
            while ((c = iter.NextCaseMapCP()) >= 0)
            {
                c = UCaseProps.INSTANCE.ToFullLower(c, iter, dest, caseLocale);
                AppendResult(c, dest, iter.CPLength, options, edits);
            }
        }

        public static string ToLower(int caseLocale, int options, string src)
        {
            return ToLower(caseLocale, options, src.ToCharSequence());
        }

        public static string ToLower(int caseLocale, int options, StringBuilder src)
        {
            return ToLower(caseLocale, options, src.ToCharSequence());
        }

        public static string ToLower(int caseLocale, int options, char[] src)
        {
            return ToLower(caseLocale, options, src.ToCharSequence());
        }

        internal static string ToLower(int caseLocale, int options, ICharSequence src)
        {
            if (src.Length <= 100 && (options & OMIT_UNCHANGED_TEXT) == 0)
            {
                if (src.Length == 0)
                {
                    return src.ToString();
                }
                // Collect and apply only changes.
                // Good if no or few changes. Bad (slow) if many changes.
                Edits edits = new Edits();
                StringBuilder replacementChars = ToLower(
                        caseLocale, options | OMIT_UNCHANGED_TEXT, src, new StringBuilder(), edits);
                return ApplyEdits(src, replacementChars, edits);
            }
            else
            {
                return ToLower(caseLocale, options, src,
                        new StringBuilder(src.Length), null).ToString();
            }
        }

        public static StringBuilder ToLower(int caseLocale, int options,
            string src, StringBuilder dest, Edits edits)
        {
            return ToLower(caseLocale, options, src.ToCharSequence(), dest, edits);
        }

        public static StringBuilder ToLower(int caseLocale, int options,
            StringBuilder src, StringBuilder dest, Edits edits)
        {
            return ToLower(caseLocale, options, src.ToCharSequence(), dest, edits);
        }

        public static StringBuilder ToLower(int caseLocale, int options,
            char[] src, StringBuilder dest, Edits edits)
        {
            return ToLower(caseLocale, options, src.ToCharSequence(), dest, edits);
        }

        internal static StringBuilder ToLower(int caseLocale, int options,
        ICharSequence src, StringBuilder dest, Edits edits)
        {
            try
            {
                if (edits != null)
                {
                    edits.Reset();
                }
                StringContextIterator iter = new StringContextIterator(src);
                InternalToLower(caseLocale, options, iter, dest, edits);
                return dest;
            }
            catch (IOException e)
            {
                throw new ICUUncheckedIOException(e);
            }
        }

        public static string ToUpper(int caseLocale, int options, string src)
        {
            return ToUpper(caseLocale, options, src.ToCharSequence());
        }

        public static string ToUpper(int caseLocale, int options, StringBuilder src)
        {
            return ToUpper(caseLocale, options, src.ToCharSequence());
        }

        public static string ToUpper(int caseLocale, int options, char[] src)
        {
            return ToUpper(caseLocale, options, src.ToCharSequence());
        }

        internal static string ToUpper(int caseLocale, int options, ICharSequence src)
        {
            if (src.Length <= 100 && (options & OMIT_UNCHANGED_TEXT) == 0)
            {
                if (src.Length == 0)
                {
                    return src.ToString();
                }
                // Collect and apply only changes.
                // Good if no or few changes. Bad (slow) if many changes.
                Edits edits = new Edits();
                StringBuilder replacementChars = ToUpper(
                        caseLocale, options | OMIT_UNCHANGED_TEXT, src, new StringBuilder(), edits);
                return ApplyEdits(src, replacementChars, edits);
            }
            else
            {
                return ToUpper(caseLocale, options, src,
                        new StringBuilder(src.Length), null).ToString();
            }
        }

        public static StringBuilder ToUpper(int caseLocale, int options,
            string src, StringBuilder dest, Edits edits)
        {
            return ToUpper(caseLocale, options, src.ToCharSequence(), dest, edits);
        }

        public static StringBuilder ToUpper(int caseLocale, int options,
            StringBuilder src, StringBuilder dest, Edits edits)
        {
            return ToUpper(caseLocale, options, src.ToCharSequence(), dest, edits);
        }

        public static StringBuilder ToUpper(int caseLocale, int options,
            char[] src, StringBuilder dest, Edits edits)
        {
            return ToUpper(caseLocale, options, src.ToCharSequence(), dest, edits);
        }

        internal static StringBuilder ToUpper(int caseLocale, int options,
        ICharSequence src, StringBuilder dest, Edits edits)
        {
            try
            {
                if (edits != null)
                {
                    edits.Reset();
                }
                if (caseLocale == UCaseProps.LOC_GREEK)
                {
                    return GreekUpper.ToUpper(options, src, dest, edits);
                }
                StringContextIterator iter = new StringContextIterator(src);
                int c;
                while ((c = iter.NextCaseMapCP()) >= 0)
                {
                    c = UCaseProps.INSTANCE.ToFullUpper(c, iter, dest, caseLocale);
                    AppendResult(c, dest, iter.CPLength, options, edits);
                }
                return dest;
            }
            catch (IOException e)
            {
                throw new ICUUncheckedIOException(e);
            }
        }

        public static string ToTitle(int caseLocale, int options, BreakIterator iter, string src)
        {
            return ToTitle(caseLocale, options, iter, src.ToCharSequence());
        }

        public static string ToTitle(int caseLocale, int options, BreakIterator iter, StringBuilder src)
        {
            return ToTitle(caseLocale, options, iter, src.ToCharSequence());
        }

        public static string ToTitle(int caseLocale, int options, BreakIterator iter, char[] src)
        {
            return ToTitle(caseLocale, options, iter, src.ToCharSequence());
        }

        internal static string ToTitle(int caseLocale, int options, BreakIterator iter, ICharSequence src)
        {
            if (src.Length <= 100 && (options & OMIT_UNCHANGED_TEXT) == 0)
            {
                if (src.Length == 0)
                {
                    return src.ToString();
                }
                // Collect and apply only changes.
                // Good if no or few changes. Bad (slow) if many changes.
                Edits edits = new Edits();
                StringBuilder replacementChars = ToTitle(
                        caseLocale, options | OMIT_UNCHANGED_TEXT, iter, src,
                        new StringBuilder(), edits);
                return ApplyEdits(src, replacementChars, edits);
            }
            else
            {
                return ToTitle(caseLocale, options, iter, src,
                        new StringBuilder(src.Length), null).ToString();
            }
        }

        public static StringBuilder ToTitle(
            int caseLocale, int options, BreakIterator titleIter,
            string src, StringBuilder dest, Edits edits)
        {
            return ToTitle(caseLocale, options, titleIter, src.ToCharSequence(), dest, edits);
        }

        public static StringBuilder ToTitle(
            int caseLocale, int options, BreakIterator titleIter,
            StringBuilder src, StringBuilder dest, Edits edits)
        {
            return ToTitle(caseLocale, options, titleIter, src.ToCharSequence(), dest, edits);
        }

        public static StringBuilder ToTitle(
            int caseLocale, int options, BreakIterator titleIter,
            char[] src, StringBuilder dest, Edits edits)
        {
            return ToTitle(caseLocale, options, titleIter, src.ToCharSequence(), dest, edits);
        }

        internal static StringBuilder ToTitle(
            int caseLocale, int options, BreakIterator titleIter,
            ICharSequence src, StringBuilder dest, Edits edits)
        {
            try
            {
                if (edits != null)
                {
                    edits.Reset();
                }

                /* set up local variables */
                StringContextIterator iter = new StringContextIterator(src);
                int srcLength = src.Length;
                int prev = 0;
                bool isFirstIndex = true;

                /* titlecasing loop */
                while (prev < srcLength)
                {
                    /* find next index where to titlecase */
                    int index;
                    if (isFirstIndex)
                    {
                        isFirstIndex = false;
                        index = titleIter.First();
                    }
                    else
                    {
                        index = titleIter.Next();
                    }
                    if (index == BreakIterator.DONE || index > srcLength)
                    {
                        index = srcLength;
                    }

                    /*
                     * Segment [prev..index[ into 3 parts:
                     * a) skipped characters (copy as-is) [prev..titleStart[
                     * b) first letter (titlecase)              [titleStart..titleLimit[
                     * c) subsequent characters (lowercase)                 [titleLimit..index[
                     */
                    if (prev < index)
                    {
                        // Find and copy skipped characters [prev..titleStart[
                        int titleStart = prev;
                        iter.SetLimit(index);
                        int c = iter.NextCaseMapCP();
                        if ((options & UCharacter.TITLECASE_NO_BREAK_ADJUSTMENT) == 0)
                        {
                            // Adjust the titlecasing index to the next cased character,
                            // or to the next letter/number/symbol/private use.
                            // Stop with titleStart<titleLimit<=index
                            // if there is a character to be titlecased,
                            // or else stop with titleStart==titleLimit==index.
                            bool toCased = (options & CaseMapImpl.TITLECASE_ADJUST_TO_CASED) != 0;
                            while ((toCased ?
                                        UCaseProps.NONE == UCaseProps.INSTANCE.GetType(c) :
                                            !CaseMapImpl.IsLNS(c)) &&
                                    (c = iter.NextCaseMapCP()) >= 0) { }
                            // If c<0 then we have only uncased characters in [prev..index[
                            // and stopped with titleStart==titleLimit==index.
                            titleStart = iter.CPStart;
                            if (prev < titleStart)
                            {
                                AppendUnchanged(src, prev, titleStart - prev, dest, options, edits);
                            }
                        }

                        if (titleStart < index)
                        {
                            int titleLimit = iter.CPLimit;
                            // titlecase c which is from [titleStart..titleLimit[
                            c = UCaseProps.INSTANCE.ToFullTitle(c, iter, dest, caseLocale);
                            AppendResult(c, dest, iter.CPLength, options, edits);

                            // Special case Dutch IJ titlecasing
                            if (titleStart + 1 < index && caseLocale == UCaseProps.LOC_DUTCH)
                            {
                                char c1 = src[titleStart];
                                if ((c1 == 'i' || c1 == 'I'))
                                {
                                    char c2 = src[titleStart + 1];
                                    if (c2 == 'j')
                                    {
                                        dest.Append('J');
                                        if (edits != null)
                                        {
                                            edits.AddReplace(1, 1);
                                        }
                                        c = iter.NextCaseMapCP();
                                        titleLimit++;
                                        Debug.Assert(c == c2);
                                        Debug.Assert(titleLimit == iter.CPLimit);
                                    }
                                    else if (c2 == 'J')
                                    {
                                        // Keep the capital J from getting lowercased.
                                        AppendUnchanged(src, titleStart + 1, 1, dest, options, edits);
                                        c = iter.NextCaseMapCP();
                                        titleLimit++;
                                        Debug.Assert(c == c2);
                                        Debug.Assert(titleLimit == iter.CPLimit);
                                    }
                                }
                            }

                            // lowercase [titleLimit..index[
                            if (titleLimit < index)
                            {
                                if ((options & UCharacter.TITLECASE_NO_LOWERCASE) == 0)
                                {
                                    // Normal operation: Lowercase the rest of the word.
                                    InternalToLower(caseLocale, options, iter, dest, edits);
                                }
                                else
                                {
                                    // Optionally just copy the rest of the word unchanged.
                                    AppendUnchanged(src, titleLimit, index - titleLimit, dest, options, edits);
                                    iter.MoveToLimit();
                                }
                            }
                        }
                    }

                    prev = index;
                }
                return dest;
            }
            catch (IOException e)
            {
                throw new ICUUncheckedIOException(e);
            }
        }

        public static string Fold(int options, string src)
        {
            return Fold(options, src.ToCharSequence());
        }

        public static string Fold(int options, StringBuilder src)
        {
            return Fold(options, src.ToCharSequence());
        }

        public static string Fold(int options, char[] src)
        {
            return Fold(options, src.ToCharSequence());
        }

        internal static string Fold(int options, ICharSequence src)
        {
            if (src.Length <= 100 && (options & OMIT_UNCHANGED_TEXT) == 0)
            {
                if (src.Length == 0)
                {
                    return src.ToString();
                }
                // Collect and apply only changes.
                // Good if no or few changes. Bad (slow) if many changes.
                Edits edits = new Edits();
                StringBuilder replacementChars = Fold(
                        options | OMIT_UNCHANGED_TEXT, src, new StringBuilder(), edits);
                return ApplyEdits(src, replacementChars, edits);
            }
            else
            {
                return Fold(options, src, new StringBuilder(src.Length), null).ToString();
            }
        }

        public static StringBuilder Fold(int options,
            string src, StringBuilder dest, Edits edits)
        {
            return Fold(options, src.ToCharSequence(), dest, edits);
        }

        public static StringBuilder Fold(int options,
            StringBuilder src, StringBuilder dest, Edits edits)
        {
            return Fold(options, src.ToCharSequence(), dest, edits);
        }

        public static StringBuilder Fold(int options,
            char[] src, StringBuilder dest, Edits edits)
        {
            return Fold(options, src.ToCharSequence(), dest, edits);
        }

        internal static StringBuilder Fold(int options,
        ICharSequence src, StringBuilder dest, Edits edits)
        {
            try
            {
                if (edits != null)
                {
                    edits.Reset();
                }
                int length = src.Length;
                for (int i = 0; i < length;)
                {
                    int c = Character.CodePointAt(src, i);
                    int cpLength = Character.CharCount(c);
                    i += cpLength;
                    c = UCaseProps.INSTANCE.ToFullFolding(c, dest, options);
                    AppendResult(c, dest, cpLength, options, edits);
                }
                return dest;
            }
            catch (IOException e)
            {
                throw new ICUUncheckedIOException(e);
            }
        }

        private sealed class GreekUpper
        {
            // Data bits.
            private static readonly int UPPER_MASK = 0x3ff;
            private static readonly int HAS_VOWEL = 0x1000;
            private static readonly int HAS_YPOGEGRAMMENI = 0x2000;
            private static readonly int HAS_ACCENT = 0x4000;
            private static readonly int HAS_DIALYTIKA = 0x8000;
            // Further bits during data building and processing, not stored in the data map.
            private static readonly int HAS_COMBINING_DIALYTIKA = 0x10000;
            private static readonly int HAS_OTHER_GREEK_DIACRITIC = 0x20000;

            private static readonly int HAS_VOWEL_AND_ACCENT = HAS_VOWEL | HAS_ACCENT;
            private static readonly int HAS_VOWEL_AND_ACCENT_AND_DIALYTIKA =
                    HAS_VOWEL_AND_ACCENT | HAS_DIALYTIKA;
            private static readonly int HAS_EITHER_DIALYTIKA = HAS_DIALYTIKA | HAS_COMBINING_DIALYTIKA;

            // State bits.
            private static readonly int AFTER_CASED = 1;
            private static readonly int AFTER_VOWEL_WITH_ACCENT = 2;

            // Data generated by prototype code, see
            // http://site.icu-project.org/design/case/greek-upper
            // TODO: Move this data into ucase.icu.
            private static readonly char[] data0370 = {
            // U+0370..03FF
            (char)0x0370,  // Ͱ
            (char)0x0370,  // ͱ
            (char)0x0372,  // Ͳ
            (char)0x0372,  // ͳ
            (char)0,
            (char)0,
            (char)0x0376,  // Ͷ
            (char)0x0376,  // ͷ
            (char)0,
            (char)0,
            (char)0x037A,  // ͺ
            (char)0x03FD,  // ͻ
            (char)0x03FE,  // ͼ
            (char)0x03FF,  // ͽ
            (char)0,
            (char)0x037F,  // Ϳ
            (char)0,
            (char)0,
            (char)0,
            (char)0,
            (char)0,
            (char)0,
            (char)(0x0391 | HAS_VOWEL | HAS_ACCENT),  // Ά
            (char)0,
            (char)(0x0395 | HAS_VOWEL | HAS_ACCENT),  // Έ
            (char)(0x0397 | HAS_VOWEL | HAS_ACCENT),  // Ή
            (char)(0x0399 | HAS_VOWEL | HAS_ACCENT),  // Ί
            (char)0,
            (char)(0x039F | HAS_VOWEL | HAS_ACCENT),  // Ό
            (char)0,
            (char)(0x03A5 | HAS_VOWEL | HAS_ACCENT),  // Ύ
            (char)(0x03A9 | HAS_VOWEL | HAS_ACCENT),  // Ώ
            (char)(0x0399 | HAS_VOWEL | HAS_ACCENT | HAS_DIALYTIKA),  // ΐ
            (char)(0x0391 | HAS_VOWEL),  // Α
            (char)0x0392,  // Β
            (char)0x0393,  // Γ
            (char)0x0394,  // Δ
            (char)(0x0395 | HAS_VOWEL),  // Ε
            (char)0x0396,  // Ζ
            (char)(0x0397 | HAS_VOWEL),  // Η
            (char)0x0398,  // Θ
            (char)(0x0399 | HAS_VOWEL),  // Ι
            (char)0x039A,  // Κ
            (char)0x039B,  // Λ
            (char)0x039C,  // Μ
            (char)0x039D,  // Ν
            (char)0x039E,  // Ξ
            (char)(0x039F | HAS_VOWEL),  // Ο
            (char)0x03A0,  // Π
            (char)0x03A1,  // Ρ
            (char)0,
            (char)0x03A3,  // Σ
            (char)0x03A4,  // Τ
            (char)(0x03A5 | HAS_VOWEL),  // Υ
            (char)0x03A6,  // Φ
            (char)0x03A7,  // Χ
            (char)0x03A8,  // Ψ
            (char)(0x03A9 | HAS_VOWEL),  // Ω
            (char)(0x0399 | HAS_VOWEL | HAS_DIALYTIKA),  // Ϊ
            (char)(0x03A5 | HAS_VOWEL | HAS_DIALYTIKA),  // Ϋ
            (char)(0x0391 | HAS_VOWEL | HAS_ACCENT),  // ά
            (char)(0x0395 | HAS_VOWEL | HAS_ACCENT),  // έ
            (char)(0x0397 | HAS_VOWEL | HAS_ACCENT),  // ή
            (char)(0x0399 | HAS_VOWEL | HAS_ACCENT),  // ί
            (char)(0x03A5 | HAS_VOWEL | HAS_ACCENT | HAS_DIALYTIKA),  // ΰ
            (char)(0x0391 | HAS_VOWEL),  // α
            (char)0x0392,  // β
            (char)0x0393,  // γ
            (char)0x0394,  // δ
            (char)(0x0395 | HAS_VOWEL),  // ε
            (char)0x0396,  // ζ
            (char)(0x0397 | HAS_VOWEL),  // η
            (char)0x0398,  // θ
            (char)(0x0399 | HAS_VOWEL),  // ι
            (char)0x039A,  // κ
            (char)0x039B,  // λ
            (char)0x039C,  // μ
            (char)0x039D,  // ν
            (char)0x039E,  // ξ
            (char)(0x039F | HAS_VOWEL),  // ο
            (char)0x03A0,  // π
            (char)0x03A1,  // ρ
            (char)0x03A3,  // ς
            (char)0x03A3,  // σ
            (char)0x03A4,  // τ
            (char)(0x03A5 | HAS_VOWEL),  // υ
            (char)0x03A6,  // φ
            (char)0x03A7,  // χ
            (char)0x03A8,  // ψ
            (char)(0x03A9 | HAS_VOWEL),  // ω
            (char)(0x0399 | HAS_VOWEL | HAS_DIALYTIKA),  // ϊ
            (char)(0x03A5 | HAS_VOWEL | HAS_DIALYTIKA),  // ϋ
            (char)(0x039F | HAS_VOWEL | HAS_ACCENT),  // ό
            (char)(0x03A5 | HAS_VOWEL | HAS_ACCENT),  // ύ
            (char)(0x03A9 | HAS_VOWEL | HAS_ACCENT),  // ώ
            (char)0x03CF,  // Ϗ
            (char)0x0392,  // ϐ
            (char)0x0398,  // ϑ
            (char)0x03D2,  // ϒ
            (char)(0x03D2 | HAS_ACCENT),  // ϓ
            (char)(0x03D2 | HAS_DIALYTIKA),  // ϔ
            (char)0x03A6,  // ϕ
            (char)0x03A0,  // ϖ
            (char)0x03CF,  // ϗ
            (char)0x03D8,  // Ϙ
            (char)0x03D8,  // ϙ
            (char)0x03DA,  // Ϛ
            (char)0x03DA,  // ϛ
            (char)0x03DC,  // Ϝ
            (char)0x03DC,  // ϝ
            (char)0x03DE,  // Ϟ
            (char)0x03DE,  // ϟ
            (char)0x03E0,  // Ϡ
            (char)0x03E0,  // ϡ
            (char)0,
            (char)0,
            (char)0,
            (char)0,
            (char)0,
            (char)0,
            (char)0,
            (char)0,
            (char)0,
            (char)0,
            (char)0,
            (char)0,
            (char)0,
            (char)0,
            (char)0x039A,  // ϰ
            (char)0x03A1,  // ϱ
            (char)0x03F9,  // ϲ
            (char)0x037F,  // ϳ
            (char)0x03F4,  // ϴ
            (char)(0x0395 | HAS_VOWEL),  // ϵ
            (char)0,
            (char)0x03F7,  // Ϸ
            (char)0x03F7,  // ϸ
            (char)0x03F9,  // Ϲ
            (char)0x03FA,  // Ϻ
            (char)0x03FA,  // ϻ
            (char)0x03FC,  // ϼ
            (char)0x03FD,  // Ͻ
            (char)0x03FE,  // Ͼ
            (char)0x03FF,  // Ͽ
        };

            private static readonly char[] data1F00 = {
            // U+1F00..1FFF
            (char)(0x0391 | HAS_VOWEL),  // ἀ
            (char)(0x0391 | HAS_VOWEL),  // ἁ
            (char)(0x0391 | HAS_VOWEL | HAS_ACCENT),  // ἂ
            (char)(0x0391 | HAS_VOWEL | HAS_ACCENT),  // ἃ
            (char)(0x0391 | HAS_VOWEL | HAS_ACCENT),  // ἄ
            (char)(0x0391 | HAS_VOWEL | HAS_ACCENT),  // ἅ
            (char)(0x0391 | HAS_VOWEL | HAS_ACCENT),  // ἆ
            (char)(0x0391 | HAS_VOWEL | HAS_ACCENT),  // ἇ
            (char)(0x0391 | HAS_VOWEL),  // Ἀ
            (char)(0x0391 | HAS_VOWEL),  // Ἁ
            (char)(0x0391 | HAS_VOWEL | HAS_ACCENT),  // Ἂ
            (char)(0x0391 | HAS_VOWEL | HAS_ACCENT),  // Ἃ
            (char)(0x0391 | HAS_VOWEL | HAS_ACCENT),  // Ἄ
            (char)(0x0391 | HAS_VOWEL | HAS_ACCENT),  // Ἅ
            (char)(0x0391 | HAS_VOWEL | HAS_ACCENT),  // Ἆ
            (char)(0x0391 | HAS_VOWEL | HAS_ACCENT),  // Ἇ
            (char)(0x0395 | HAS_VOWEL),  // ἐ
            (char)(0x0395 | HAS_VOWEL),  // ἑ
            (char)(0x0395 | HAS_VOWEL | HAS_ACCENT),  // ἒ
            (char)(0x0395 | HAS_VOWEL | HAS_ACCENT),  // ἓ
            (char)(0x0395 | HAS_VOWEL | HAS_ACCENT),  // ἔ
            (char)(0x0395 | HAS_VOWEL | HAS_ACCENT),  // ἕ
            (char)0,
            (char)0,
            (char)(0x0395 | HAS_VOWEL),  // Ἐ
            (char)(0x0395 | HAS_VOWEL),  // Ἑ
            (char)(0x0395 | HAS_VOWEL | HAS_ACCENT),  // Ἒ
            (char)(0x0395 | HAS_VOWEL | HAS_ACCENT),  // Ἓ
            (char)(0x0395 | HAS_VOWEL | HAS_ACCENT),  // Ἔ
            (char)(0x0395 | HAS_VOWEL | HAS_ACCENT),  // Ἕ
            (char)0,
            (char)0,
            (char)(0x0397 | HAS_VOWEL),  // ἠ
            (char)(0x0397 | HAS_VOWEL),  // ἡ
            (char)(0x0397 | HAS_VOWEL | HAS_ACCENT),  // ἢ
            (char)(0x0397 | HAS_VOWEL | HAS_ACCENT),  // ἣ
            (char)(0x0397 | HAS_VOWEL | HAS_ACCENT),  // ἤ
            (char)(0x0397 | HAS_VOWEL | HAS_ACCENT),  // ἥ
            (char)(0x0397 | HAS_VOWEL | HAS_ACCENT),  // ἦ
            (char)(0x0397 | HAS_VOWEL | HAS_ACCENT),  // ἧ
            (char)(0x0397 | HAS_VOWEL),  // Ἠ
            (char)(0x0397 | HAS_VOWEL),  // Ἡ
            (char)(0x0397 | HAS_VOWEL | HAS_ACCENT),  // Ἢ
            (char)(0x0397 | HAS_VOWEL | HAS_ACCENT),  // Ἣ
            (char)(0x0397 | HAS_VOWEL | HAS_ACCENT),  // Ἤ
            (char)(0x0397 | HAS_VOWEL | HAS_ACCENT),  // Ἥ
            (char)(0x0397 | HAS_VOWEL | HAS_ACCENT),  // Ἦ
            (char)(0x0397 | HAS_VOWEL | HAS_ACCENT),  // Ἧ
            (char)(0x0399 | HAS_VOWEL),  // ἰ
            (char)(0x0399 | HAS_VOWEL),  // ἱ
            (char)(0x0399 | HAS_VOWEL | HAS_ACCENT),  // ἲ
            (char)(0x0399 | HAS_VOWEL | HAS_ACCENT),  // ἳ
            (char)(0x0399 | HAS_VOWEL | HAS_ACCENT),  // ἴ
            (char)(0x0399 | HAS_VOWEL | HAS_ACCENT),  // ἵ
            (char)(0x0399 | HAS_VOWEL | HAS_ACCENT),  // ἶ
            (char)(0x0399 | HAS_VOWEL | HAS_ACCENT),  // ἷ
            (char)(0x0399 | HAS_VOWEL),  // Ἰ
            (char)(0x0399 | HAS_VOWEL),  // Ἱ
            (char)(0x0399 | HAS_VOWEL | HAS_ACCENT),  // Ἲ
            (char)(0x0399 | HAS_VOWEL | HAS_ACCENT),  // Ἳ
            (char)(0x0399 | HAS_VOWEL | HAS_ACCENT),  // Ἴ
            (char)(0x0399 | HAS_VOWEL | HAS_ACCENT),  // Ἵ
            (char)(0x0399 | HAS_VOWEL | HAS_ACCENT),  // Ἶ
            (char)(0x0399 | HAS_VOWEL | HAS_ACCENT),  // Ἷ
            (char)(0x039F | HAS_VOWEL),  // ὀ
            (char)(0x039F | HAS_VOWEL),  // ὁ
            (char)(0x039F | HAS_VOWEL | HAS_ACCENT),  // ὂ
            (char)(0x039F | HAS_VOWEL | HAS_ACCENT),  // ὃ
            (char)(0x039F | HAS_VOWEL | HAS_ACCENT),  // ὄ
            (char)(0x039F | HAS_VOWEL | HAS_ACCENT),  // ὅ
            (char)0,
            (char)0,
            (char)(0x039F | HAS_VOWEL),  // Ὀ
            (char)(0x039F | HAS_VOWEL),  // Ὁ
            (char)(0x039F | HAS_VOWEL | HAS_ACCENT),  // Ὂ
            (char)(0x039F | HAS_VOWEL | HAS_ACCENT),  // Ὃ
            (char)(0x039F | HAS_VOWEL | HAS_ACCENT),  // Ὄ
            (char)(0x039F | HAS_VOWEL | HAS_ACCENT),  // Ὅ
            (char)0,
            (char)0,
            (char)(0x03A5 | HAS_VOWEL),  // ὐ
            (char)(0x03A5 | HAS_VOWEL),  // ὑ
            (char)(0x03A5 | HAS_VOWEL | HAS_ACCENT),  // ὒ
            (char)(0x03A5 | HAS_VOWEL | HAS_ACCENT),  // ὓ
            (char)(0x03A5 | HAS_VOWEL | HAS_ACCENT),  // ὔ
            (char)(0x03A5 | HAS_VOWEL | HAS_ACCENT),  // ὕ
            (char)(0x03A5 | HAS_VOWEL | HAS_ACCENT),  // ὖ
            (char)(0x03A5 | HAS_VOWEL | HAS_ACCENT),  // ὗ
            (char)0,
            (char)(0x03A5 | HAS_VOWEL),  // Ὑ
            (char)0,
            (char)(0x03A5 | HAS_VOWEL | HAS_ACCENT),  // Ὓ
            (char)0,
            (char)(0x03A5 | HAS_VOWEL | HAS_ACCENT),  // Ὕ
            (char)0,
            (char)(0x03A5 | HAS_VOWEL | HAS_ACCENT),  // Ὗ
            (char)(0x03A9 | HAS_VOWEL),  // ὠ
            (char)(0x03A9 | HAS_VOWEL),  // ὡ
            (char)(0x03A9 | HAS_VOWEL | HAS_ACCENT),  // ὢ
            (char)(0x03A9 | HAS_VOWEL | HAS_ACCENT),  // ὣ
            (char)(0x03A9 | HAS_VOWEL | HAS_ACCENT),  // ὤ
            (char)(0x03A9 | HAS_VOWEL | HAS_ACCENT),  // ὥ
            (char)(0x03A9 | HAS_VOWEL | HAS_ACCENT),  // ὦ
            (char)(0x03A9 | HAS_VOWEL | HAS_ACCENT),  // ὧ
            (char)(0x03A9 | HAS_VOWEL),  // Ὠ
            (char)(0x03A9 | HAS_VOWEL),  // Ὡ
            (char)(0x03A9 | HAS_VOWEL | HAS_ACCENT),  // Ὢ
            (char)(0x03A9 | HAS_VOWEL | HAS_ACCENT),  // Ὣ
            (char)(0x03A9 | HAS_VOWEL | HAS_ACCENT),  // Ὤ
            (char)(0x03A9 | HAS_VOWEL | HAS_ACCENT),  // Ὥ
            (char)(0x03A9 | HAS_VOWEL | HAS_ACCENT),  // Ὦ
            (char)(0x03A9 | HAS_VOWEL | HAS_ACCENT),  // Ὧ
            (char)(0x0391 | HAS_VOWEL | HAS_ACCENT),  // ὰ
            (char)(0x0391 | HAS_VOWEL | HAS_ACCENT),  // ά
            (char)(0x0395 | HAS_VOWEL | HAS_ACCENT),  // ὲ
            (char)(0x0395 | HAS_VOWEL | HAS_ACCENT),  // έ
            (char)(0x0397 | HAS_VOWEL | HAS_ACCENT),  // ὴ
            (char)(0x0397 | HAS_VOWEL | HAS_ACCENT),  // ή
            (char)(0x0399 | HAS_VOWEL | HAS_ACCENT),  // ὶ
            (char)(0x0399 | HAS_VOWEL | HAS_ACCENT),  // ί
            (char)(0x039F | HAS_VOWEL | HAS_ACCENT),  // ὸ
            (char)(0x039F | HAS_VOWEL | HAS_ACCENT),  // ό
            (char)(0x03A5 | HAS_VOWEL | HAS_ACCENT),  // ὺ
            (char)(0x03A5 | HAS_VOWEL | HAS_ACCENT),  // ύ
            (char)(0x03A9 | HAS_VOWEL | HAS_ACCENT),  // ὼ
            (char)(0x03A9 | HAS_VOWEL | HAS_ACCENT),  // ώ
            (char)0,
            (char)0,
            (char)(0x0391 | HAS_VOWEL | HAS_YPOGEGRAMMENI),  // ᾀ
            (char)(0x0391 | HAS_VOWEL | HAS_YPOGEGRAMMENI),  // ᾁ
            (char)(0x0391 | HAS_VOWEL | HAS_YPOGEGRAMMENI | HAS_ACCENT),  // ᾂ
            (char)(0x0391 | HAS_VOWEL | HAS_YPOGEGRAMMENI | HAS_ACCENT),  // ᾃ
            (char)(0x0391 | HAS_VOWEL | HAS_YPOGEGRAMMENI | HAS_ACCENT),  // ᾄ
            (char)(0x0391 | HAS_VOWEL | HAS_YPOGEGRAMMENI | HAS_ACCENT),  // ᾅ
            (char)(0x0391 | HAS_VOWEL | HAS_YPOGEGRAMMENI | HAS_ACCENT),  // ᾆ
            (char)(0x0391 | HAS_VOWEL | HAS_YPOGEGRAMMENI | HAS_ACCENT),  // ᾇ
            (char)(0x0391 | HAS_VOWEL | HAS_YPOGEGRAMMENI),  // ᾈ
            (char)(0x0391 | HAS_VOWEL | HAS_YPOGEGRAMMENI),  // ᾉ
            (char)(0x0391 | HAS_VOWEL | HAS_YPOGEGRAMMENI | HAS_ACCENT),  // ᾊ
            (char)(0x0391 | HAS_VOWEL | HAS_YPOGEGRAMMENI | HAS_ACCENT),  // ᾋ
            (char)(0x0391 | HAS_VOWEL | HAS_YPOGEGRAMMENI | HAS_ACCENT),  // ᾌ
            (char)(0x0391 | HAS_VOWEL | HAS_YPOGEGRAMMENI | HAS_ACCENT),  // ᾍ
            (char)(0x0391 | HAS_VOWEL | HAS_YPOGEGRAMMENI | HAS_ACCENT),  // ᾎ
            (char)(0x0391 | HAS_VOWEL | HAS_YPOGEGRAMMENI | HAS_ACCENT),  // ᾏ
            (char)(0x0397 | HAS_VOWEL | HAS_YPOGEGRAMMENI),  // ᾐ
            (char)(0x0397 | HAS_VOWEL | HAS_YPOGEGRAMMENI),  // ᾑ
            (char)(0x0397 | HAS_VOWEL | HAS_YPOGEGRAMMENI | HAS_ACCENT),  // ᾒ
            (char)(0x0397 | HAS_VOWEL | HAS_YPOGEGRAMMENI | HAS_ACCENT),  // ᾓ
            (char)(0x0397 | HAS_VOWEL | HAS_YPOGEGRAMMENI | HAS_ACCENT),  // ᾔ
            (char)(0x0397 | HAS_VOWEL | HAS_YPOGEGRAMMENI | HAS_ACCENT),  // ᾕ
            (char)(0x0397 | HAS_VOWEL | HAS_YPOGEGRAMMENI | HAS_ACCENT),  // ᾖ
            (char)(0x0397 | HAS_VOWEL | HAS_YPOGEGRAMMENI | HAS_ACCENT),  // ᾗ
            (char)(0x0397 | HAS_VOWEL | HAS_YPOGEGRAMMENI),  // ᾘ
            (char)(0x0397 | HAS_VOWEL | HAS_YPOGEGRAMMENI),  // ᾙ
            (char)(0x0397 | HAS_VOWEL | HAS_YPOGEGRAMMENI | HAS_ACCENT),  // ᾚ
            (char)(0x0397 | HAS_VOWEL | HAS_YPOGEGRAMMENI | HAS_ACCENT),  // ᾛ
            (char)(0x0397 | HAS_VOWEL | HAS_YPOGEGRAMMENI | HAS_ACCENT),  // ᾜ
            (char)(0x0397 | HAS_VOWEL | HAS_YPOGEGRAMMENI | HAS_ACCENT),  // ᾝ
            (char)(0x0397 | HAS_VOWEL | HAS_YPOGEGRAMMENI | HAS_ACCENT),  // ᾞ
            (char)(0x0397 | HAS_VOWEL | HAS_YPOGEGRAMMENI | HAS_ACCENT),  // ᾟ
            (char)(0x03A9 | HAS_VOWEL | HAS_YPOGEGRAMMENI),  // ᾠ
            (char)(0x03A9 | HAS_VOWEL | HAS_YPOGEGRAMMENI),  // ᾡ
            (char)(0x03A9 | HAS_VOWEL | HAS_YPOGEGRAMMENI | HAS_ACCENT),  // ᾢ
            (char)(0x03A9 | HAS_VOWEL | HAS_YPOGEGRAMMENI | HAS_ACCENT),  // ᾣ
            (char)(0x03A9 | HAS_VOWEL | HAS_YPOGEGRAMMENI | HAS_ACCENT),  // ᾤ
            (char)(0x03A9 | HAS_VOWEL | HAS_YPOGEGRAMMENI | HAS_ACCENT),  // ᾥ
            (char)(0x03A9 | HAS_VOWEL | HAS_YPOGEGRAMMENI | HAS_ACCENT),  // ᾦ
            (char)(0x03A9 | HAS_VOWEL | HAS_YPOGEGRAMMENI | HAS_ACCENT),  // ᾧ
            (char)(0x03A9 | HAS_VOWEL | HAS_YPOGEGRAMMENI),  // ᾨ
            (char)(0x03A9 | HAS_VOWEL | HAS_YPOGEGRAMMENI),  // ᾩ
            (char)(0x03A9 | HAS_VOWEL | HAS_YPOGEGRAMMENI | HAS_ACCENT),  // ᾪ
            (char)(0x03A9 | HAS_VOWEL | HAS_YPOGEGRAMMENI | HAS_ACCENT),  // ᾫ
            (char)(0x03A9 | HAS_VOWEL | HAS_YPOGEGRAMMENI | HAS_ACCENT),  // ᾬ
            (char)(0x03A9 | HAS_VOWEL | HAS_YPOGEGRAMMENI | HAS_ACCENT),  // ᾭ
            (char)(0x03A9 | HAS_VOWEL | HAS_YPOGEGRAMMENI | HAS_ACCENT),  // ᾮ
            (char)(0x03A9 | HAS_VOWEL | HAS_YPOGEGRAMMENI | HAS_ACCENT),  // ᾯ
            (char)(0x0391 | HAS_VOWEL),  // ᾰ
            (char)(0x0391 | HAS_VOWEL),  // ᾱ
            (char)(0x0391 | HAS_VOWEL | HAS_YPOGEGRAMMENI | HAS_ACCENT),  // ᾲ
            (char)(0x0391 | HAS_VOWEL | HAS_YPOGEGRAMMENI),  // ᾳ
            (char)(0x0391 | HAS_VOWEL | HAS_YPOGEGRAMMENI | HAS_ACCENT),  // ᾴ
            (char)0,
            (char)(0x0391 | HAS_VOWEL | HAS_ACCENT),  // ᾶ
            (char)(0x0391 | HAS_VOWEL | HAS_YPOGEGRAMMENI | HAS_ACCENT),  // ᾷ
            (char)(0x0391 | HAS_VOWEL),  // Ᾰ
            (char)(0x0391 | HAS_VOWEL),  // Ᾱ
            (char)(0x0391 | HAS_VOWEL | HAS_ACCENT),  // Ὰ
            (char)(0x0391 | HAS_VOWEL | HAS_ACCENT),  // Ά
            (char)(0x0391 | HAS_VOWEL | HAS_YPOGEGRAMMENI),  // ᾼ
            (char)0,
            (char)(0x0399 | HAS_VOWEL),  // ι
            (char)0,
            (char)0,
            (char)0,
            (char)(0x0397 | HAS_VOWEL | HAS_YPOGEGRAMMENI | HAS_ACCENT),  // ῂ
            (char)(0x0397 | HAS_VOWEL | HAS_YPOGEGRAMMENI),  // ῃ
            (char)(0x0397 | HAS_VOWEL | HAS_YPOGEGRAMMENI | HAS_ACCENT),  // ῄ
            (char)0,
            (char)(0x0397 | HAS_VOWEL | HAS_ACCENT),  // ῆ
            (char)(0x0397 | HAS_VOWEL | HAS_YPOGEGRAMMENI | HAS_ACCENT),  // ῇ
            (char)(0x0395 | HAS_VOWEL | HAS_ACCENT),  // Ὲ
            (char)(0x0395 | HAS_VOWEL | HAS_ACCENT),  // Έ
            (char)(0x0397 | HAS_VOWEL | HAS_ACCENT),  // Ὴ
            (char)(0x0397 | HAS_VOWEL | HAS_ACCENT),  // Ή
            (char)(0x0397 | HAS_VOWEL | HAS_YPOGEGRAMMENI),  // ῌ
            (char)0,
            (char)0,
            (char)0,
            (char)(0x0399 | HAS_VOWEL),  // ῐ
            (char)(0x0399 | HAS_VOWEL),  // ῑ
            (char)(0x0399 | HAS_VOWEL | HAS_ACCENT | HAS_DIALYTIKA),  // ῒ
            (char)(0x0399 | HAS_VOWEL | HAS_ACCENT | HAS_DIALYTIKA),  // ΐ
            (char)0,
            (char)0,
            (char)(0x0399 | HAS_VOWEL | HAS_ACCENT),  // ῖ
            (char)(0x0399 | HAS_VOWEL | HAS_ACCENT | HAS_DIALYTIKA),  // ῗ
            (char)(0x0399 | HAS_VOWEL),  // Ῐ
            (char)(0x0399 | HAS_VOWEL),  // Ῑ
            (char)(0x0399 | HAS_VOWEL | HAS_ACCENT),  // Ὶ
            (char)(0x0399 | HAS_VOWEL | HAS_ACCENT),  // Ί
            (char)0,
            (char)0,
            (char)0,
            (char)0,
            (char)(0x03A5 | HAS_VOWEL),  // ῠ
            (char)(0x03A5 | HAS_VOWEL),  // ῡ
            (char)(0x03A5 | HAS_VOWEL | HAS_ACCENT | HAS_DIALYTIKA),  // ῢ
            (char)(0x03A5 | HAS_VOWEL | HAS_ACCENT | HAS_DIALYTIKA),  // ΰ
            (char)0x03A1,  // ῤ
            (char)0x03A1,  // ῥ
            (char)(0x03A5 | HAS_VOWEL | HAS_ACCENT),  // ῦ
            (char)(0x03A5 | HAS_VOWEL | HAS_ACCENT | HAS_DIALYTIKA),  // ῧ
            (char)(0x03A5 | HAS_VOWEL),  // Ῠ
            (char)(0x03A5 | HAS_VOWEL),  // Ῡ
            (char)(0x03A5 | HAS_VOWEL | HAS_ACCENT),  // Ὺ
            (char)(0x03A5 | HAS_VOWEL | HAS_ACCENT),  // Ύ
            (char)0x03A1,  // Ῥ
            (char)0,
            (char)0,
            (char)0,
            (char)0,
            (char)0,
            (char)(0x03A9 | HAS_VOWEL | HAS_YPOGEGRAMMENI | HAS_ACCENT),  // ῲ
            (char)(0x03A9 | HAS_VOWEL | HAS_YPOGEGRAMMENI),  // ῳ
            (char)(0x03A9 | HAS_VOWEL | HAS_YPOGEGRAMMENI | HAS_ACCENT),  // ῴ
            (char)0,
            (char)(0x03A9 | HAS_VOWEL | HAS_ACCENT),  // ῶ
            (char)(0x03A9 | HAS_VOWEL | HAS_YPOGEGRAMMENI | HAS_ACCENT),  // ῷ
            (char)(0x039F | HAS_VOWEL | HAS_ACCENT),  // Ὸ
            (char)(0x039F | HAS_VOWEL | HAS_ACCENT),  // Ό
            (char)(0x03A9 | HAS_VOWEL | HAS_ACCENT),  // Ὼ
            (char)(0x03A9 | HAS_VOWEL | HAS_ACCENT),  // Ώ
            (char)(0x03A9 | HAS_VOWEL | HAS_YPOGEGRAMMENI),  // ῼ
            (char)0,
            (char)0,
            (char)0,
        };

            // U+2126 Ohm sign
            private static readonly char data2126 = (char)(0x03A9 | HAS_VOWEL);  // Ω

            private static int GetLetterData(int c)
            {
                if (c < 0x370 || 0x2126 < c || (0x3ff < c && c < 0x1f00))
                {
                    return 0;
                }
                else if (c <= 0x3ff)
                {
                    return data0370[c - 0x370];
                }
                else if (c <= 0x1fff)
                {
                    return data1F00[c - 0x1f00];
                }
                else if (c == 0x2126)
                {
                    return data2126;
                }
                else
                {
                    return 0;
                }
            }

            /**
             * Returns a non-zero value for each of the Greek combining diacritics
             * listed in The Unicode Standard, version 8, chapter 7.2 Greek,
             * plus some perispomeni look-alikes.
             */
            private static int GetDiacriticData(int c)
            {
                switch (c)
                {
                    case '\u0300':  // varia
                    case '\u0301':  // tonos = oxia
                    case '\u0342':  // perispomeni
                    case '\u0302':  // circumflex can look like perispomeni
                    case '\u0303':  // tilde can look like perispomeni
                    case '\u0311':  // inverted breve can look like perispomeni
                        return HAS_ACCENT;
                    case '\u0308':  // dialytika = diaeresis
                        return HAS_COMBINING_DIALYTIKA;
                    case '\u0344':  // dialytika tonos
                        return HAS_COMBINING_DIALYTIKA | HAS_ACCENT;
                    case '\u0345':  // ypogegrammeni = iota subscript
                        return HAS_YPOGEGRAMMENI;
                    case '\u0304':  // macron
                    case '\u0306':  // breve
                    case '\u0313':  // comma above
                    case '\u0314':  // reversed comma above
                    case '\u0343':  // koronis
                        return HAS_OTHER_GREEK_DIACRITIC;
                    default:
                        return 0;
                }
            }

            private static bool IsFollowedByCasedLetter(ICharSequence s, int i)
            {
                while (i < s.Length)
                {
                    int c = Character.CodePointAt(s, i);
                    int type = UCaseProps.INSTANCE.GetTypeOrIgnorable(c);
                    if ((type & UCaseProps.IGNORABLE) != 0)
                    {
                        // Case-ignorable, continue with the loop.
                        i += Character.CharCount(c);
                    }
                    else if (type != UCaseProps.NONE)
                    {
                        return true;  // Followed by cased letter.
                    }
                    else
                    {
                        return false;  // Uncased and not case-ignorable.
                    }
                }
                return false;  // Not followed by cased letter.
            }

            /**
             * Greek string uppercasing with a state machine.
             * Probably simpler than a stateless function that has to figure out complex context-before
             * for each character.
             * TODO: Try to re-consolidate one way or another with the non-Greek function.
             *
             * <p>Keep this consistent with the C++ versions in ustrcase.cpp (UTF-16) and ucasemap.cpp (UTF-8).
             * @throws IOException
             */
            internal static StringBuilder ToUpper(int options,
                    ICharSequence src, StringBuilder dest, Edits edits)
            {
                int state = 0;
                for (int i = 0; i < src.Length;)
                {
                    int c = Character.CodePointAt(src, i);
                    int nextIndex = i + Character.CharCount(c);
                    int nextState = 0;
                    int type = UCaseProps.INSTANCE.GetTypeOrIgnorable(c);
                    if ((type & UCaseProps.IGNORABLE) != 0)
                    {
                        // c is case-ignorable
                        nextState |= (state & AFTER_CASED);
                    }
                    else if (type != UCaseProps.NONE)
                    {
                        // c is cased
                        nextState |= AFTER_CASED;
                    }
                    int data = GetLetterData(c);
                    if (data > 0)
                    {
                        int upper = data & UPPER_MASK;
                        // Add a dialytika to this iota or ypsilon vowel
                        // if we removed a tonos from the previous vowel,
                        // and that previous vowel did not also have (or gain) a dialytika.
                        // Adding one only to the final vowel in a longer sequence
                        // (which does not occur in normal writing) would require lookahead.
                        // Set the same flag as for preserving an existing dialytika.
                        if ((data & HAS_VOWEL) != 0 && (state & AFTER_VOWEL_WITH_ACCENT) != 0 &&
                                (upper == 'Ι' || upper == 'Υ'))
                        {
                            data |= HAS_DIALYTIKA;
                        }
                        int numYpogegrammeni = 0;  // Map each one to a trailing, spacing, capital iota.
                        if ((data & HAS_YPOGEGRAMMENI) != 0)
                        {
                            numYpogegrammeni = 1;
                        }
                        // Skip combining diacritics after this Greek letter.
                        while (nextIndex < src.Length)
                        {
                            int diacriticData = GetDiacriticData(src[nextIndex]);
                            if (diacriticData != 0)
                            {
                                data |= diacriticData;
                                if ((diacriticData & HAS_YPOGEGRAMMENI) != 0)
                                {
                                    ++numYpogegrammeni;
                                }
                                ++nextIndex;
                            }
                            else
                            {
                                break;  // not a Greek diacritic
                            }
                        }
                        if ((data & HAS_VOWEL_AND_ACCENT_AND_DIALYTIKA) == HAS_VOWEL_AND_ACCENT)
                        {
                            nextState |= AFTER_VOWEL_WITH_ACCENT;
                        }
                        // Map according to Greek rules.
                        bool addTonos = false;
                        if (upper == 'Η' &&
                                (data & HAS_ACCENT) != 0 &&
                                numYpogegrammeni == 0 &&
                                (state & AFTER_CASED) == 0 &&
                                !IsFollowedByCasedLetter(src, nextIndex))
                        {
                            // Keep disjunctive "or" with (only) a tonos.
                            // We use the same "word boundary" conditions as for the Final_Sigma test.
                            if (i == nextIndex)
                            {
                                upper = 'Ή';  // Preserve the precomposed form.
                            }
                            else
                            {
                                addTonos = true;
                            }
                        }
                        else if ((data & HAS_DIALYTIKA) != 0)
                        {
                            // Preserve a vowel with dialytika in precomposed form if it exists.
                            if (upper == 'Ι')
                            {
                                upper = 'Ϊ';
                                data &= ~HAS_EITHER_DIALYTIKA;
                            }
                            else if (upper == 'Υ')
                            {
                                upper = 'Ϋ';
                                data &= ~HAS_EITHER_DIALYTIKA;
                            }
                        }

                        bool change;
                        if (edits == null && (options & OMIT_UNCHANGED_TEXT) == 0)
                        {
                            change = true;  // common, simple usage
                        }
                        else
                        {
                            // Find out first whether we are changing the text.
                            change = src[i] != upper || numYpogegrammeni > 0;
                            int i2 = i + 1;
                            if ((data & HAS_EITHER_DIALYTIKA) != 0)
                            {
                                change |= i2 >= nextIndex || src[i2] != 0x308;
                                ++i2;
                            }
                            if (addTonos)
                            {
                                change |= i2 >= nextIndex || src[i2] != 0x301;
                                ++i2;
                            }
                            int oldLength = nextIndex - i;
                            int newLength = (i2 - i) + numYpogegrammeni;
                            change |= oldLength != newLength;
                            if (change)
                            {
                                if (edits != null)
                                {
                                    edits.AddReplace(oldLength, newLength);
                                }
                            }
                            else
                            {
                                if (edits != null)
                                {
                                    edits.AddUnchanged(oldLength);
                                }
                                // Write unchanged text?
                                change = (options & OMIT_UNCHANGED_TEXT) == 0;
                            }
                        }

                        if (change)
                        {
                            dest.Append((char)upper);
                            if ((data & HAS_EITHER_DIALYTIKA) != 0)
                            {
                                dest.Append('\u0308');  // restore or add a dialytika
                            }
                            if (addTonos)
                            {
                                dest.Append('\u0301');
                            }
                            while (numYpogegrammeni > 0)
                            {
                                dest.Append('Ι');
                                --numYpogegrammeni;
                            }
                        }
                    }
                    else
                    {
                        c = UCaseProps.INSTANCE.ToFullUpper(c, null, dest, UCaseProps.LOC_GREEK);
                        AppendResult(c, dest, nextIndex - i, options, edits);
                    }
                    i = nextIndex;
                    state = nextState;
                }
                return dest;
            }
        }
    }
}
