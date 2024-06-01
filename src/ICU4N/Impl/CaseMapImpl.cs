using ICU4N.Globalization;
using ICU4N.Support.Text;
using ICU4N.Text;
using J2N;
using J2N.Text;
using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
#nullable enable

namespace ICU4N.Impl
{
    public sealed partial class CaseMapImpl
    {
        private const int CharStackBufferSize = 32;

        internal const int U_SENTINEL = -1;

        // ICU4N: De-nested StringContextIterator

        public const int TitleCaseWholeString = 0x20;  // ICU4N TODO: API Change to [Flags] enum
        public const int TitleCaseSentences = 0x40;  // ICU4N TODO: API Change to [Flags] enum

        /// <summary>
        /// Bit mask for the titlecasing iterator options bit field.
        /// Currently only 3 out of 8 values are used:
        /// 0 (words), <see cref="TitleCaseWholeString"/>, <see cref="TitleCaseSentences"/>.
        /// See stringoptions.h.
        /// </summary>
        /// <internal/>
        private const int TITLECASE_ITERATOR_MASK = 0xe0;  // ICU4N TODO: API Change to [Flags] enum, Rename to follow .NET Conventions

        public const int TitleCaseAdjustToCased = 0x400;  // ICU4N TODO: API Change to [Flags] enum

        /// <summary>
        /// Bit mask for the titlecasing index adjustment options bit set.
        /// Currently two bits are defined:
        /// <see cref="UChar.TitleCaseNoBreakAdjustment"/>, <see cref="TitleCaseAdjustToCased"/>.
        /// See stringoptions.h.
        /// </summary>
        /// <internal/>
        private const int TITLECASE_ADJUSTMENT_MASK = 0x600; // ICU4N TODO: API Change to [Flags] enum, Rename to follow .NET Conventions

        public static int AddTitleAdjustmentOption(int options, int newOption)
        {
            int adjOptions = options & TITLECASE_ADJUSTMENT_MASK;
            if (adjOptions != 0 && adjOptions != newOption)
            {
                throw new ArgumentException("multiple titlecasing index adjustment options");
            }
            return options | newOption;
        }

        private const int LNS =
                (1 << (int)UUnicodeCategory.UppercaseLetter) |
                (1 << (int)UUnicodeCategory.LowercaseLetter) |
                (1 << (int)UUnicodeCategory.TitlecaseLetter) |
                // Not MODIFIER_LETTER: We count only cased modifier letters.
                (1 << (int)UUnicodeCategory.OtherLetter) |

                (1 << (int)UUnicodeCategory.DecimalDigitNumber) |
                (1 << (int)UUnicodeCategory.LetterNumber) |
                (1 << (int)UUnicodeCategory.OtherNumber) |

                (1 << (int)UUnicodeCategory.MathSymbol) |
                (1 << (int)UUnicodeCategory.CurrencySymbol) |
                (1 << (int)UUnicodeCategory.ModifierSymbol) |
                (1 << (int)UUnicodeCategory.OtherSymbol) |

                (1 << (int)UUnicodeCategory.PrivateUse);

        private static bool IsLNS(int c)
        {
            // Letter, number, symbol,
            // or a private use code point because those are typically used as letters or numbers.
            // Consider modifier letters only if they are cased.
            UUnicodeCategory gc = UCharacterProperty.Instance.GetUnicodeCategory(c);
            return ((1 << (int)gc) & LNS) != 0 ||
                    (gc == UUnicodeCategory.ModifierLetter &&
                        UCaseProperties.Instance.GetCaseType(c) != CaseType.None);
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
            CultureInfo? locale, int options, BreakIterator? iter)
        {
            options &= TITLECASE_ITERATOR_MASK;
            if (options != 0 && iter != null)
            {
                throw new ArgumentException(
                        "titlecasing iterator option together with an explicit iterator");
            }
            // ICU4N: added guard clause so we don't get a NullReferenceException
            if (options != TitleCaseWholeString && locale is null && iter is null)
                throw new ArgumentException($"Either {nameof(locale)} or {nameof(iter)} must be non-null when {nameof(TitleCaseWholeString)} is not used.");
            if (iter is null)
            {
                switch (options)
                {
                    case 0:
                        iter = BreakIterator.GetWordInstance(locale!);
                        break;
                    case TitleCaseWholeString:
                        iter = new WholeStringBreakIterator();
                        break;
                    case TitleCaseSentences:
                        iter = BreakIterator.GetSentenceInstance(locale!);
                        break;
                    default:
                        throw new ArgumentException("unknown titlecasing iterator option");
                }
            }
            return iter;
        }

        public static BreakIterator GetTitleBreakIterator(
            UCultureInfo? locale, int options, BreakIterator? iter)
        {
            options &= TITLECASE_ITERATOR_MASK;
            if (options != 0 && iter != null)
            {
                throw new ArgumentException(
                        "titlecasing iterator option together with an explicit iterator");
            }
            // ICU4N: added guard clause so we don't get a NullReferenceException
            if (options != TitleCaseWholeString && locale is null && iter is null)
                throw new ArgumentException($"Either {nameof(locale)} or {nameof(iter)} must be non-null when {nameof(TitleCaseWholeString)} is not used.");

            if (iter is null)
            {
                switch (options)
                {
                    case 0:
                        iter = BreakIterator.GetWordInstance(locale!);
                        break;
                    case TitleCaseWholeString:
                        iter = new WholeStringBreakIterator();
                        break;
                    case TitleCaseSentences:
                        iter = BreakIterator.GetSentenceInstance(locale!);
                        break;
                    default:
                        throw new ArgumentException("unknown titlecasing iterator option");
                }
            }
            return iter;
        }

        /// <summary>
        /// Omit unchanged text when case-mapping with Edits.
        /// </summary>
        public const int OmitUnchangedText = 0x4000; // ICU4N TODO: API - make into Flags enum

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

            public override CharacterIterator? Text
            {
                get
                {
                    NotImplemented();
                    return null;
                }
            }

            public override void SetText(CharacterIterator newText)
            {
                length = newText.EndIndex;
            }

            public override void SetText(ReadOnlyMemory<char> newText)
            {
                length = newText.Length;
            }

            public override void SetText(string newText)
            {
                length = newText.Length;
            }
        }

        // ICU4N specific - AppendCodePoint(IAppendable a, int c) - use ValueStringBuilder.AppendCodePoint() instead.

        /// <summary>
        /// Appends a full case mapping result, see <see cref="UCaseProperties.MaxStringLength"/>
        /// </summary>
        private static void AppendResult(int result, ref ValueStringBuilder dest,
            int cpLength, int options, Edits? edits)
        {
            // Decode the result.
            if (result < 0)
            {
                // (not) original code point
                if (edits != null)
                {
                    edits.AddUnchanged(cpLength);
                }
                if ((options & OmitUnchangedText) != 0)
                {
                    return;
                }
                dest.AppendCodePoint(~result);
            }
            else if (result <= UCaseProperties.MaxStringLength)
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
                int length = dest.AppendCodePoint(result);
                if (edits != null)
                {
                    edits.AddReplace(cpLength, length);
                }
            }
        }

        private unsafe static void AppendUnchanged(char* src, int start, int length,
            ref ValueStringBuilder dest, int options, Edits? edits)
        {
            if (length > 0)
            {
                if (edits != null)
                {
                    edits.AddUnchanged(length);
                }
                if ((options & OmitUnchangedText) != 0)
                {
                    return;
                }
                dest.Append(src + start, length);
            }
        }

        // ICU4N: Ported from ustrcase.cpp utf16_caseContextIterator

        // ICU4N: In C++ context was a void*, but in C# we use IntPtr to allow for managed types to be used.
        private unsafe static int Utf16CaseContextIterator(IntPtr context, sbyte dir)
        {
            UCaseContext* csc = (UCaseContext*)context;
            int c;

            if (dir < 0)
            {
                /* reset for backward iteration */
                csc->index = csc->cpStart;
                csc->dir = dir;
            }
            else if (dir > 0)
            {
                /* reset for forward iteration */
                csc->index = csc->cpLimit;
                csc->dir = dir;
            }
            else
            {
                /* continue current iteration direction */
                dir = csc->dir;
            }

            if (dir < 0)
            {
                if (csc->start < csc->index)
                {
                    UTF16.Previous((char*)csc->p, csc->start, ref csc->index, out c);
                    return c;
                }
            }
            else
            {
                if (csc->index < csc->limit)
                {
                    UTF16.Next((char*)csc->p, ref csc->index, csc->limit, out c);
                    return c;
                }
            }
            return U_SENTINEL;
        }



        // ICU4N specific - ApplyEdits(ICharSequence src, StringBuilder replacementChars, Edits edits) 
        // moved to CaseMapImpl.generated.tt

        private static void ApplyEdits(ReadOnlySpan<char> src, ReadOnlySpan<char> replacementChars, Edits edits, ref ValueStringBuilder result)
        {
            if (!edits.HasChanges)
            {
                result.Append(src);
                return;
            }
            for (EditsEnumerator ei = edits.GetCoarseEnumerator(); ei.MoveNext();)
            {
                if (ei.HasChange)
                {
                    int i = ei.ReplacementIndex;
                    result.Append(replacementChars.Slice(i, ei.NewLength)); // ICU4N: (i + ei.NewLength) - i == ei.NewLength
                }
                else
                {
                    int i = ei.SourceIndex;
                    result.Append(src.Slice(i, ei.OldLength)); // ICU4N: (i + ie.OldLength) - i == ie.OldLength
                }
            }
        }

        // ICU4N: Ported this from the ustrcase.cpp file. We don't want to be heap bound, so we are using a pointer
        // which allows the use of a ref struct for the context parameter. _CaseMap does the actual iteration.
        private unsafe static void InternalToLower(CaseLocale caseLocale, int options,
            ref ValueStringBuilder dest, char* src, int srcLength, Edits? edits)
        {
            UCaseContext csc = new UCaseContext();
            csc.p = (IntPtr)src; // ICU4N: In C++ this was a void*, but in C# we use IntPtr to allow for managed types to be used.
            csc.limit = srcLength;
            _CaseMap(
                caseLocale, options, UCaseProperties.Instance.ToFullLower,
                ref dest, src, &csc, srcStart: 0, srcLength,
                edits);
        }

        // ICU4N: Ported this from the ustrcase.cpp file. We don't want to be heap bound, so we are using a pointer
        // which allows the use of a ref struct for the context parameter. _CaseMap does the actual iteration.
        private unsafe static void InternalToUpper(CaseLocale caseLocale, int options,
            ref ValueStringBuilder dest, char* src, int srcLength, Edits? edits)
        {
            if (caseLocale == CaseLocale.Greek)
            {
                GreekUpper.ToUpper(options, ref dest, src, srcLength, edits);
                return;
            }
            UCaseContext csc = new UCaseContext();
            csc.p = (IntPtr)src; // ICU4N: In C++ this was a void*, but in C# we use IntPtr to allow for managed types to be used.
            csc.limit = srcLength;
            _CaseMap(
                caseLocale, options, UCaseProperties.Instance.ToFullUpper,
                ref dest, src, &csc, srcStart: 0, srcLength,
                edits);
        }

        // ICU4N: Ported this from the ustrcase.cpp file. We don't want to be heap bound, so we are using a pointer
        // which allows the use of a ref struct for the context parameter. _CaseMap does the actual iteration.
        internal unsafe static void InternalToTitle(
            CaseLocale caseLocale, int options, BreakIterator titleIter,
            ref ValueStringBuilder dest,
            char* src, int srcLength, Edits? edits)
        {
            // ICU4N TODO: This check was done in the C++ code, but not in Java. Not sure what message to throw.
            //if ((options & TITLECASE_ADJUSTMENT_MASK) == TITLECASE_ADJUSTMENT_MASK)
            //{
            //    throw new IcuArgumentException("");
            //}

            /* set up local variables */
            UCaseContext csc = new UCaseContext();
            csc.p = (IntPtr)src; // ICU4N: In C++ this was a void*, but in C# we use IntPtr to allow for managed types to be used.
            csc.limit = srcLength;
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
                if (index == BreakIterator.Done || index > srcLength)
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
                    int titleLimit = prev;
                    int c;
                    UTF16.Next(src, ref titleLimit, index, out c);
                    if ((options & UChar.TitleCaseNoBreakAdjustment) == 0)
                    {
                        // Adjust the titlecasing index to the next cased character,
                        // or to the next letter/number/symbol/private use.
                        // Stop with titleStart<titleLimit<=index
                        // if there is a character to be titlecased,
                        // or else stop with titleStart==titleLimit==index.
                        bool toCased = (options & CaseMapImpl.TitleCaseAdjustToCased) != 0;
                        while (toCased ? CaseType.None == UCaseProperties.Instance.GetCaseType(c) : !CaseMapImpl.IsLNS(c))
                        {
                            titleStart = titleLimit;
                            if (titleLimit == index)
                                break;
                            UTF16.Next(src, ref titleLimit, index, out c);
                        }
                        // If c<0 then we have only uncased characters in [prev..index[
                        // and stopped with titleStart==titleLimit==index.
                        //titleStart = iter.CPStart;
                        if (prev < titleStart)
                        {
                            AppendUnchanged(src, prev, titleStart - prev, ref dest, options, edits);
                        }
                    }

                    if (titleStart < titleLimit)
                    {
                        // titlecase c which is from [titleStart..titleLimit[
                        c = UCaseProperties.Instance.ToFullTitle(c, Utf16CaseContextIterator, (IntPtr)(&csc), ref dest, caseLocale);
                        AppendResult(c, ref dest, titleLimit - titleStart, options, edits);

                        // Special case Dutch IJ titlecasing
                        if (titleStart + 1 < index && caseLocale == CaseLocale.Dutch)
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
                                    titleLimit++;
                                }
                                else if (c2 == 'J')
                                {
                                    // Keep the capital J from getting lowercased.
                                    AppendUnchanged(src, titleStart + 1, 1, ref dest, options, edits);
                                    titleLimit++;
                                }
                            }
                        }

                        // lowercase [titleLimit..index[
                        if (titleLimit < index)
                        {
                            if ((options & UChar.TitleCaseNoLowerCase) == 0)
                            {
                                // Normal operation: Lowercase the rest of the word.
                                _CaseMap(
                                    caseLocale, options, UCaseProperties.Instance.ToFullLower,
                                    ref dest,
                                    src, &csc,
                                    titleLimit, index,
                                    edits);
                            }
                            else
                            {
                                // Optionally just copy the rest of the word unchanged.
                                AppendUnchanged(src, titleLimit, index - titleLimit, ref dest, options, edits);
                            }
                        }
                    }
                }

                prev = index;
            }
        }

        public static string ToLower(CaseLocale caseLocale, int options, ReadOnlySpan<char> src)
        {
            int length = src.Length;
            if (length <= 100 && (options & OmitUnchangedText) == 0)
            {
                if (length == 0)
                {
                    return string.Empty;
                }
                // Collect and apply only changes.
                // Good if no or few changes. Bad (slow) if many changes.
                ValueStringBuilder replacementChars = length <= CharStackBufferSize
                    ? new ValueStringBuilder(stackalloc char[CharStackBufferSize])
                    : new ValueStringBuilder(length);
                try
                {
                    Edits edits = new Edits();
                    ToLower(caseLocale, options | OmitUnchangedText, src, ref replacementChars, edits);

                    int newLength = length + edits.LengthDelta;
                    ValueStringBuilder result = newLength <= CharStackBufferSize
                    ? new ValueStringBuilder(stackalloc char[newLength])
                    : new ValueStringBuilder(newLength);
                    ApplyEdits(src, replacementChars.AsSpan(), edits, ref result);
                    return result.ToString();
                }
                finally
                {
                    replacementChars.Dispose();
                }
            }
            else
            {
                ValueStringBuilder result = length <= CharStackBufferSize
                    ? new ValueStringBuilder(stackalloc char[CharStackBufferSize])
                    : new ValueStringBuilder(length);
                ToLower(caseLocale, options, src, ref result, null);
                return result.ToString();
            }
        }

        // ICU4N specific overload
        // charsLength will return either the number of characters that were copied, or on failure, will return the number of chars to allocate to execute successfully.
        public static bool ToLower(CaseLocale caseLocale, int options, ReadOnlySpan<char> source, Span<char> destination, out int charsLength) // ICU4N TODO: Tests
        {
            int length = source.Length;
            if (length <= 100 && (options & OmitUnchangedText) == 0)
            {
                if (length == 0)
                {
                    charsLength = 0;
                    return true;
                }
                // Collect and apply only changes.
                // Good if no or few changes. Bad (slow) if many changes.
                ValueStringBuilder replacementChars = length <= CharStackBufferSize
                    ? new ValueStringBuilder(stackalloc char[CharStackBufferSize])
                    : new ValueStringBuilder(length);
                try
                {
                    Edits edits = new Edits();
                    ToLower(caseLocale, options | OmitUnchangedText, source, ref replacementChars, edits);

                    int newLength = length + edits.LengthDelta;
                    ValueStringBuilder result = newLength <= CharStackBufferSize
                        ? new ValueStringBuilder(stackalloc char[newLength])
                        : new ValueStringBuilder(newLength);
                    ApplyEdits(source, replacementChars.AsSpan(), edits, ref result);
                    charsLength = result.Length;
                    return result.TryCopyTo(destination, out _);
                }
                finally
                {
                    replacementChars.Dispose();
                }
            }
            else
            {
                ValueStringBuilder result = length <= CharStackBufferSize
                    ? new ValueStringBuilder(stackalloc char[CharStackBufferSize])
                    : new ValueStringBuilder(length);
                ToLower(caseLocale, options, source, ref result, null);
                charsLength = result.Length;
                return result.TryCopyTo(destination, out _);
            }
        }

        public static bool ToLower(CaseLocale caseLocale, int options,
            ReadOnlySpan<char> source, Span<char> destination, out int charsLength, Edits? edits) // ICU4N TODO: Tests
        {
            if (edits is null)
            {
                return ToLower(caseLocale, options, source, destination, out charsLength);
            }

            int length = source.Length;
            var sb = length <= CharStackBufferSize
                ? new ValueStringBuilder(stackalloc char[CharStackBufferSize])
                : new ValueStringBuilder(length);

            ToLower(caseLocale, options, source, ref sb, edits);
            charsLength = sb.Length;
            return sb.TryCopyTo(destination, out _);
        }

        public static StringBuilder ToLower(CaseLocale caseLocale, int options,
            ReadOnlySpan<char> src, StringBuilder dest, Edits? edits) // ICU4N TODO: API - we probably don't want this overload - we should write to Span<char> instead
        {
            if (dest is null)
                throw new ArgumentNullException(nameof(dest));

            var sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                ToLower(caseLocale, options, src, ref sb, edits);
                dest.Append(sb.AsSpan());
                return dest;
            }
            finally
            {
                sb.Dispose();
            }
        }

        public static T ToLower<T>(CaseLocale caseLocale, int options,
            ReadOnlySpan<char> src, T dest, Edits? edits) where T : IAppendable // ICU4N TODO: API - we probably don't want this overload - we should write to Span<char> instead
        {
            if (dest is null)
                throw new ArgumentNullException(nameof(dest));

            var sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                ToLower(caseLocale, options, src, ref sb, edits);
                dest.Append(sb.AsSpan());
                return dest;
            }
            finally
            {
                sb.Dispose();
            }
        }

        internal unsafe static void ToLower(CaseLocale caseLocale, int options,
            ReadOnlySpan<char> src, ref ValueStringBuilder dest, Edits? edits)
        {
            // ICU4N: Removed unnecessary try/catch
            if (edits != null)
            {
                edits.Reset();
            }
            fixed (char* srcPtr = &MemoryMarshal.GetReference(src))
            {
                InternalToLower(caseLocale, options, ref dest, srcPtr, src.Length, edits);
            }
        }

        public static string ToUpper(CaseLocale caseLocale, int options, ReadOnlySpan<char> src)
        {
            int length = src.Length;
            if (length <= 100 && (options & OmitUnchangedText) == 0)
            {
                if (length == 0)
                {
                    return string.Empty;
                }
                // Collect and apply only changes.
                // Good if no or few changes. Bad (slow) if many changes.
                ValueStringBuilder replacementChars = length <= CharStackBufferSize
                    ? new ValueStringBuilder(stackalloc char[CharStackBufferSize])
                    : new ValueStringBuilder(length);
                try
                {
                    Edits edits = new Edits();
                    ToUpper(caseLocale, options | OmitUnchangedText, src, ref replacementChars, edits);

                    int newLength = length + edits.LengthDelta;
                    ValueStringBuilder result = newLength <= CharStackBufferSize
                        ? new ValueStringBuilder(stackalloc char[newLength])
                        : new ValueStringBuilder(newLength);
                    ApplyEdits(src, replacementChars.AsSpan(), edits, ref result);
                    return result.ToString();
                }
                finally
                {
                    replacementChars.Dispose();
                }
            }
            else
            {
                ValueStringBuilder result = length <= CharStackBufferSize
                    ? new ValueStringBuilder(stackalloc char[CharStackBufferSize])
                    : new ValueStringBuilder(length);
                ToUpper(caseLocale, options, src, ref result, null);
                return result.ToString();
            }
        }

        // ICU4N specific overload
        // charsLength will return either the number of characters that were copied, or on failure, will return the number of chars to allocate to execute successfully.
        public static bool ToUpper(CaseLocale caseLocale, int options, ReadOnlySpan<char> source, Span<char> destination, out int charsLength) // ICU4N TODO: Tests
        {
            int length = source.Length;
            if (length <= 100 && (options & OmitUnchangedText) == 0)
            {
                if (length == 0)
                {
                    charsLength = 0;
                    return true;
                }
                // Collect and apply only changes.
                // Good if no or few changes. Bad (slow) if many changes.
                ValueStringBuilder replacementChars = length <= CharStackBufferSize
                    ? new ValueStringBuilder(stackalloc char[CharStackBufferSize])
                    : new ValueStringBuilder(length);
                try
                {
                    Edits edits = new Edits();
                    ToUpper(caseLocale, options | OmitUnchangedText, source, ref replacementChars, edits);

                    int newLength = length + edits.LengthDelta;
                    ValueStringBuilder result = newLength <= CharStackBufferSize
                        ? new ValueStringBuilder(stackalloc char[newLength])
                        : new ValueStringBuilder(newLength);
                    ApplyEdits(source, replacementChars.AsSpan(), edits, ref result);
                    charsLength = result.Length;
                    return result.TryCopyTo(destination, out _);
                }
                finally
                {
                    replacementChars.Dispose();
                }
            }
            else
            {
                ValueStringBuilder result = length <= CharStackBufferSize
                    ? new ValueStringBuilder(stackalloc char[CharStackBufferSize])
                    : new ValueStringBuilder(length);
                ToUpper(caseLocale, options, source, ref result, null);
                charsLength = result.Length;
                return result.TryCopyTo(destination, out _);
            }
        }

        public static bool ToUpper(CaseLocale caseLocale, int options,
            ReadOnlySpan<char> source, Span<char> destination, out int charsLength, Edits? edits) // ICU4N TODO: Tests
        {
            if (edits is null)
            {
                return ToUpper(caseLocale, options, source, destination, out charsLength);
            }

            int length = source.Length;
            var sb = length <= CharStackBufferSize
                ? new ValueStringBuilder(stackalloc char[CharStackBufferSize])
                : new ValueStringBuilder(length);

            ToUpper(caseLocale, options, source, ref sb, edits);
            charsLength = sb.Length;
            return sb.TryCopyTo(destination, out _);
        }

        public static StringBuilder ToUpper(CaseLocale caseLocale, int options,
            ReadOnlySpan<char> src, StringBuilder dest, Edits? edits) // ICU4N TODO: API - we probably don't want this overload - we should write to Span<char> instead
        {
            if (dest is null)
                throw new ArgumentNullException(nameof(dest));

            var sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                ToUpper(caseLocale, options, src, ref sb, edits);
                dest.Append(sb.AsSpan());
                return dest;
            }
            finally
            {
                sb.Dispose();
            }
        }

        public static T ToUpper<T>(CaseLocale caseLocale, int options,
            ReadOnlySpan<char> src, T dest, Edits? edits) where T : IAppendable // ICU4N TODO: API - we probably don't want this overload - we should write to Span<char> instead
        {
            if (dest is null)
                throw new ArgumentNullException(nameof(dest));

            var sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                ToUpper(caseLocale, options, src, ref sb, edits);
                dest.Append(sb.AsSpan());
                return dest;
            }
            finally
            {
                sb.Dispose();
            }
        }

        internal unsafe static void ToUpper(CaseLocale caseLocale, int options,
            ReadOnlySpan<char> src, ref ValueStringBuilder dest, Edits? edits)
        {
            // ICU4N: Removed unnecessary try/catch
            if (edits != null)
            {
                edits.Reset();
            }
            fixed (char* srcPtr = &MemoryMarshal.GetReference(src))
            {
                InternalToUpper(caseLocale, options, ref dest, srcPtr, src.Length, edits);
            }
        }

        public static string ToTitle(CaseLocale caseLocale, int options, BreakIterator iter, ReadOnlySpan<char> src)
        {
            int length = src.Length;
            if (length <= 100 && (options & OmitUnchangedText) == 0)
            {
                if (length == 0)
                {
                    return string.Empty;
                }
                // Collect and apply only changes.
                // Good if no or few changes. Bad (slow) if many changes.
                ValueStringBuilder replacementChars = length <= CharStackBufferSize
                    ? new ValueStringBuilder(stackalloc char[CharStackBufferSize])
                    : new ValueStringBuilder(length);
                try
                {
                    Edits edits = new Edits();
                    ToTitle(caseLocale, options | OmitUnchangedText, iter, src, ref replacementChars, edits);

                    int newLength = length + edits.LengthDelta;
                    ValueStringBuilder result = newLength <= CharStackBufferSize
                    ? new ValueStringBuilder(stackalloc char[newLength])
                    : new ValueStringBuilder(newLength);
                    ApplyEdits(src, replacementChars.AsSpan(), edits, ref result);
                    return result.ToString();
                }
                finally
                {
                    replacementChars.Dispose();
                }
            }
            else
            {
                ValueStringBuilder result = length <= CharStackBufferSize
                    ? new ValueStringBuilder(stackalloc char[CharStackBufferSize])
                    : new ValueStringBuilder(length);
                ToTitle(caseLocale, options, iter, src, ref result, null);
                return result.ToString();
            }
        }

        // ICU4N specific overload
        // charsLength will return either the number of characters that were copied, or on failure, will return the number of chars to allocate to execute successfully.
        public static bool ToTitle(CaseLocale caseLocale, int options, BreakIterator iter, ReadOnlySpan<char> source, Span<char> destination, out int charsLength) // ICU4N TODO: Tests
        {
            int length = source.Length;
            if (length <= 100 && (options & OmitUnchangedText) == 0)
            {
                if (length == 0)
                {
                    charsLength = 0;
                    return true;
                }
                // Collect and apply only changes.
                // Good if no or few changes. Bad (slow) if many changes.
                ValueStringBuilder replacementChars = length <= CharStackBufferSize
                    ? new ValueStringBuilder(stackalloc char[CharStackBufferSize])
                    : new ValueStringBuilder(length);
                try
                {
                    Edits edits = new Edits();
                    ToTitle(caseLocale, options | OmitUnchangedText, iter, source, ref replacementChars, edits);

                    int newLength = length + edits.LengthDelta;
                    ValueStringBuilder result = newLength <= CharStackBufferSize
                        ? new ValueStringBuilder(stackalloc char[newLength])
                        : new ValueStringBuilder(newLength);
                    ApplyEdits(source, replacementChars.AsSpan(), edits, ref result);
                    charsLength = result.Length;
                    return result.TryCopyTo(destination, out _);
                }
                finally
                {
                    replacementChars.Dispose();
                }
            }
            else
            {
                ValueStringBuilder result = length <= CharStackBufferSize
                    ? new ValueStringBuilder(stackalloc char[CharStackBufferSize])
                    : new ValueStringBuilder(length);
                ToTitle(caseLocale, options, iter, source, ref result, null);
                charsLength = result.Length;
                return result.TryCopyTo(destination, out _);
            }
        }

        public static bool ToTitle(
            CaseLocale caseLocale, int options, BreakIterator titleIter,
            ReadOnlySpan<char> source, Span<char> destination, out int charsLength, Edits? edits) // ICU4N TODO: Tests
        {
            if (edits is null)
            {
                return ToTitle(caseLocale, options, titleIter, source, destination, out charsLength);
            }

            int length = source.Length;
            var sb = length <= CharStackBufferSize
                ? new ValueStringBuilder(stackalloc char[CharStackBufferSize])
                : new ValueStringBuilder(length);

            ToTitle(caseLocale, options, titleIter, source, ref sb, edits);
            charsLength = sb.Length;
            return sb.TryCopyTo(destination, out _);
        }

        public static StringBuilder ToTitle(
            CaseLocale caseLocale, int options, BreakIterator titleIter,
            ReadOnlySpan<char> src, StringBuilder dest, Edits? edits) // ICU4N TODO: API - we probably don't want this overload - we should write to Span<char> instead
        {
            if (dest is null)
                throw new ArgumentNullException(nameof(dest));

            var sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                ToTitle(caseLocale, options, titleIter, src, ref sb, edits);
                dest.Append(sb.AsSpan());
                return dest;
            }
            finally
            {
                sb.Dispose();
            }
        }

        public static T ToTitle<T>(
            CaseLocale caseLocale, int options, BreakIterator titleIter,
            ReadOnlySpan<char> src, T dest, Edits? edits) where T: IAppendable // ICU4N TODO: API - we probably don't want this overload - we should write to Span<char> instead
        {
            if (dest is null)
                throw new ArgumentNullException(nameof(dest));

            var sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                ToTitle(caseLocale, options, titleIter, src, ref sb, edits);
                dest.Append(sb.AsSpan());
                return dest;
            }
            finally
            {
                sb.Dispose();
            }
        }

        internal unsafe static void ToTitle(
            CaseLocale caseLocale, int options, BreakIterator titleIter,
            ReadOnlySpan<char> src, ref ValueStringBuilder dest, Edits? edits)
        {
            if (titleIter is null)
                throw new ArgumentNullException(nameof(titleIter));

            // ICU4N: Removed unnecessary try/catch
            if (edits != null)
            {
                edits.Reset();
            }
            fixed (char* srcPtr = &MemoryMarshal.GetReference(src))
            {
                InternalToTitle(caseLocale, options, titleIter, ref dest, srcPtr, src.Length, edits);
            }
        }

        public static string Fold(int options, ReadOnlySpan<char> src)
        {
            int length = src.Length;
            if (length <= 100 && (options & OmitUnchangedText) == 0)
            {
                if (length == 0)
                {
                    return string.Empty;
                }
                // Collect and apply only changes.
                // Good if no or few changes. Bad (slow) if many changes.
                ValueStringBuilder replacementChars = length <= CharStackBufferSize
                    ? new ValueStringBuilder(stackalloc char[CharStackBufferSize])
                    : new ValueStringBuilder(length);
                try
                {
                    Edits edits = new Edits();
                    Fold(options | OmitUnchangedText, src, ref replacementChars, edits);

                    int newLength = length + edits.LengthDelta;
                    ValueStringBuilder result = newLength <= CharStackBufferSize
                    ? new ValueStringBuilder(stackalloc char[newLength])
                    : new ValueStringBuilder(newLength);
                    ApplyEdits(src, replacementChars.AsSpan(), edits, ref result);
                    return result.ToString();
                }
                finally
                {
                    replacementChars.Dispose();
                }
            }
            else
            {
                ValueStringBuilder result = length <= CharStackBufferSize
                    ? new ValueStringBuilder(stackalloc char[CharStackBufferSize])
                    : new ValueStringBuilder(length);
                Fold(options, src, ref result, null);
                return result.ToString();
            }
        }

        // ICU4N specific overload
        // charsLength will return either the number of characters that were copied, or on failure, will return the number of chars to allocate to execute successfully.
        public static bool Fold(int options, ReadOnlySpan<char> source, Span<char> destination, out int charsLength) // ICU4N TODO: Tests
        {
            int length = source.Length;
            if (length <= 100 && (options & OmitUnchangedText) == 0)
            {
                if (length == 0)
                {
                    charsLength = 0;
                    return true;
                }
                // Collect and apply only changes.
                // Good if no or few changes. Bad (slow) if many changes.
                ValueStringBuilder replacementChars = length <= CharStackBufferSize
                    ? new ValueStringBuilder(stackalloc char[CharStackBufferSize])
                    : new ValueStringBuilder(length);
                try
                {
                    Edits edits = new Edits();
                    Fold(options | OmitUnchangedText, source, ref replacementChars, edits);

                    int newLength = length + edits.LengthDelta;
                    ValueStringBuilder result = newLength <= CharStackBufferSize
                        ? new ValueStringBuilder(stackalloc char[newLength])
                        : new ValueStringBuilder(newLength);
                    ApplyEdits(source, replacementChars.AsSpan(), edits, ref result);
                    charsLength = result.Length;
                    return result.TryCopyTo(destination, out _);
                }
                finally
                {
                    replacementChars.Dispose();
                }
            }
            else
            {
                ValueStringBuilder result = length <= CharStackBufferSize
                    ? new ValueStringBuilder(stackalloc char[CharStackBufferSize])
                    : new ValueStringBuilder(length);
                Fold(options, source, ref result, null);
                charsLength = result.Length;
                return result.TryCopyTo(destination, out _);
            }
        }

        public static bool Fold(int options,
            ReadOnlySpan<char> source, Span<char> destination, out int charsLength, Edits? edits) // ICU4N TODO: Tests
        {
            if (edits is null)
            {
                return Fold(options, source, destination, out charsLength);
            }

            int length = source.Length;
            var sb = length <= CharStackBufferSize
                ? new ValueStringBuilder(stackalloc char[CharStackBufferSize])
                : new ValueStringBuilder(length);

            Fold(options, source, ref sb, edits);
            charsLength = sb.Length;
            return sb.TryCopyTo(destination, out _);
        }

        public static StringBuilder Fold(int options,
            ReadOnlySpan<char> src, StringBuilder dest, Edits? edits)
        {
            if (dest is null)
                throw new ArgumentNullException(nameof(dest));

            var sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                Fold(options, src, ref sb, edits);
                dest.Append(sb.AsSpan());
                return dest;
            }
            finally
            {
                sb.Dispose();
            }
        }

        public static T Fold<T>(int options,
            ReadOnlySpan<char> src, T dest, Edits? edits) where T : IAppendable
        {
            if (dest is null)
                throw new ArgumentNullException(nameof(dest));

            var sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                Fold(options, src, ref sb, edits);
                dest.Append(sb.AsSpan());
                return dest;
            }
            finally
            {
                sb.Dispose();
            }
        }

        internal static void Fold(int options,
            ReadOnlySpan<char> src, ref ValueStringBuilder dest, Edits? edits)
        {
            // ICU4N: Removed unnecessary try/catch
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
                c = UCaseProperties.Instance.ToFullFolding(c, ref dest, options);
                AppendResult(c, ref dest, cpLength, options, edits);
            }
        }

        /*
         * Case-maps [srcStart..srcLimit[ but takes
         * context [0..srcLength[ into account.
         */
        // ICU4N: Ported this from the ustrcase.cpp file. We don't want to be heap bound, so we are using a pointer
        // which allows the use of a ref struct for the context parameter.
        private unsafe static void _CaseMap(CaseLocale caseLocale, int options, UCaseMapFull map,
            ref ValueStringBuilder dest, char* src, UCaseContext* csc, int srcStart, int srcLimit, Edits? edits)
        {
            /* case mapping loop */
            int srcIndex = srcStart;
            while (srcIndex < srcLimit)
            {
                int cpStart;
                csc->cpStart = cpStart = srcIndex;
                UTF16.Next(src, ref srcIndex, srcLimit, out int c);
                csc->cpLimit = srcIndex;
                c = map(c, Utf16CaseContextIterator, (IntPtr)csc, ref dest, caseLocale);
                AppendResult(c, ref dest, srcIndex - cpStart, options, edits);
            }
        }

        private sealed partial class GreekUpper
        {
            // Data bits.
            private const int UPPER_MASK = 0x3ff;
            private const int HAS_VOWEL = 0x1000;
            private const int HAS_YPOGEGRAMMENI = 0x2000;
            private const int HAS_ACCENT = 0x4000;
            private const int HAS_DIALYTIKA = 0x8000;
            // Further bits during data building and processing, not stored in the data map.
            private const int HAS_COMBINING_DIALYTIKA = 0x10000;
            private const int HAS_OTHER_GREEK_DIACRITIC = 0x20000;

            private const int HAS_VOWEL_AND_ACCENT = HAS_VOWEL | HAS_ACCENT;
            private const int HAS_VOWEL_AND_ACCENT_AND_DIALYTIKA =
                    HAS_VOWEL_AND_ACCENT | HAS_DIALYTIKA;
            private const int HAS_EITHER_DIALYTIKA = HAS_DIALYTIKA | HAS_COMBINING_DIALYTIKA;

            // State bits.
            private const int AFTER_CASED = 1;
            private const int AFTER_VOWEL_WITH_ACCENT = 2;

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
            private const char data2126 = (char)(0x03A9 | HAS_VOWEL);  // Ω

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

            /// <summary>
            /// Returns a non-zero value for each of the Greek combining diacritics
            /// listed in The Unicode Standard, version 8, chapter 7.2 Greek,
            /// plus some perispomeni look-alikes.
            /// </summary>
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

            // ICU4N specific - IsFollowedByCasedLetter(ICharSequence s, int i) moved to CaseMapImpl.generated.tt

            private unsafe static bool IsFollowedByCasedLetter(char* s, int i, int length)
            {
                while (i < length)
                {
                    UTF16.Next(s, ref i, length, out int c);
                    // ICU4N: Simplfied version of GetTypeOrIgnorable
                    if (UCaseProperties.Instance.IsCaseIgnorable(c, out CaseType type))
                    {
                        // Case-ignorable, continue with the loop.
                    }
                    else if (type != CaseType.None)
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

            /// <summary>
            /// Greek string uppercasing with a state machine.
            /// Probably simpler than a stateless function that has to figure out complex context-before
            /// for each character.
            /// <para/>
            /// TODO: Try to re-consolidate one way or another with the non-Greek function.
            /// <para/>
            /// Keep this consistent with the C++ versions in ustrcase.cpp (UTF-16) and ucasemap.cpp (UTF-8).
            /// </summary>
            public unsafe static void ToUpper(int options,
                ref ValueStringBuilder dest,
                char* src, int srcLength, Edits? edits)
            {
                int state = 0;
                for (int i = 0; i < srcLength;)
                {
                    int nextIndex = i;
                    int c;
                    UTF16.Next(src, ref nextIndex, srcLength, out c);
                    int nextState = 0;
                    // ICU4N: Simplfied version of GetTypeOrIgnorable
                    if (UCaseProperties.Instance.IsCaseIgnorable(c, out CaseType type))
                    {
                        // c is case-ignorable
                        nextState |= (state & AFTER_CASED);
                    }
                    else if (type != CaseType.None)
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
                        while (nextIndex < srcLength)
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
                                !IsFollowedByCasedLetter(src, nextIndex, srcLength))
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
                        if (edits == null && (options & OmitUnchangedText) == 0)
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
                                change = (options & OmitUnchangedText) == 0;
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
                        c = UCaseProperties.Instance.ToFullUpper(c, null, IntPtr.Zero, ref dest, CaseLocale.Greek);
                        AppendResult(c, ref dest, nextIndex - i, options, edits);
                    }
                    i = nextIndex;
                    state = nextState;
                }
            }
        }
    }
}
