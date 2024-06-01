using ICU4N.Globalization;
using ICU4N.Impl;
using ICU4N.Support.Text;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace ICU4N.Text
{
    /// <summary>
    /// A transliterator that converts all letters (as defined by
    /// <see cref="UChar.IsLetter(int)"/>) to lower case, except for those
    /// letters preceded by non-letters.  The latter are converted to title
    /// case using <see cref="UChar.ToTitleCase(int)"/>.
    /// </summary>
    /// <author>Alan Liu</author>
    internal class TitlecaseTransliterator : Transliterator
    {
        private readonly object syncLock = new object();
        internal const string _ID = "Any-Title";
        // TODO: Add variants for tr/az, lt, default = default locale: ICU ticket #12720

        /// <summary>
        /// System registration hook.
        /// </summary>
        internal static void Register()
        {
            TitlecaseTransliteratorFactory.Register();
            RegisterSpecialInverse("Title", "Lower", false);
        }

        private sealed class TitlecaseTransliteratorFactory : ITransliteratorFactory
        {
            public static void Register()
                => RegisterFactory(_ID, new TitlecaseTransliteratorFactory());

            public Transliterator GetInstance(string id)
                => new TitlecaseTransliterator(new UCultureInfo("en_US"));
        }

        private readonly UCultureInfo locale;

        private readonly UCaseProperties csp;
        private CaseLocale caseLocale;

        /// <summary>
        /// Constructs a transliterator.
        /// </summary>
        public TitlecaseTransliterator(UCultureInfo loc)
            : base(_ID, null)
        {
            locale = loc;
            // Need to look back 2 characters in the case of "can't"
            MaximumContextLength = 2;
            csp = UCaseProperties.Instance;
            caseLocale = UCaseProperties.GetCaseLocale(locale);
        }

        /// <summary>
        /// Implements <see cref="Transliterator.HandleTransliterate(IReplaceable, TransliterationPosition, bool)"/>.
        /// </summary>
        protected override void HandleTransliterate(IReplaceable text,
                                           TransliterationPosition offsets, bool isIncremental)
        {
            lock (syncLock)
            {
                // TODO reimplement, see ustrcase.c
                // using a real word break iterator
                //   instead of just looking for a transition between cased and uncased characters
                // call CaseMapTransliterator::handleTransliterate() for lowercasing? (set fMap)
                // needs to take isIncremental into account because case mappings are context-sensitive
                //   also detect when lowercasing function did not finish because of context

                if (offsets.Start >= offsets.Limit)
                {
                    return;
                }

                // case type: >0 cased (UCaseProps.LOWER etc.)  ==0 uncased  <0 case-ignorable
                CaseType type;

                // Our mode; we are either converting letter toTitle or
                // toLower.
                bool doTitle = true;

                // Determine if there is a preceding context of cased case-ignorable*,
                // in which case we want to start in toLower mode.  If the
                // prior context is anything else (including empty) then start
                // in toTitle mode.
                int c, start;
                for (start = offsets.Start - 1; start >= offsets.ContextStart; start -= UTF16.GetCharCount(c))
                {
                    c = text.Char32At(start);
                    // ICU4N: Simplfied version of GetTypeOrIgnorable
                    if (!csp.IsCaseIgnorable(c, out type))
                    {
                        if (type > 0)
                        { // cased
                            doTitle = false;
                            break;
                        }
                        else if (type == 0)
                        { // uncased but not ignorable
                            break;
                        }
                    }
                    // else case-ignorable: continue
                }

                // Convert things after a cased character toLower; things
                // after a uncased, non-case-ignorable character toTitle.  Case-ignorable
                // characters are copied directly and do not change the mode.
                UCaseContext csc = new UCaseContext();
                GCHandle handle = GCHandle.Alloc(text, GCHandleType.Normal);
                try
                {
                    csc.p = GCHandle.ToIntPtr(handle);
                    csc.start = offsets.ContextStart;
                    csc.limit = offsets.ContextLimit;

                    ValueStringBuilder replacementChars = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);

                    int textPos, delta = 0, result;

                    Span<char> codePointBuffer = stackalloc char[2];

                    // Walk through original string
                    // If there is a case change, modify corresponding position in replaceable
                    for (textPos = offsets.Start; textPos < offsets.Limit;)
                    {
                        csc.cpStart = textPos;
                        c = text.Char32At(textPos);
                        csc.cpLimit = textPos += UTF16.GetCharCount(c);

                        // ICU4N: Simplfied version of GetTypeOrIgnorable
                        if (!csp.IsCaseIgnorable(c, out type))
                        {// not case-ignorable
                            unsafe
                            {
                                if (doTitle)
                                {
                                    result = csp.ToFullTitle(c, Replaceable.CaseContextIterator, (IntPtr)(&csc), ref replacementChars, caseLocale);
                                }
                                else
                                {
                                    result = csp.ToFullLower(c, Replaceable.CaseContextIterator, (IntPtr)(&csc), ref replacementChars, caseLocale);
                                }
                            }
                            doTitle = type == CaseType.None; // doTitle=isUncased

                            if (csc.b1 && isIncremental)
                            {
                                // fMap() tried to look beyond the context limit
                                // wait for more input
                                offsets.Start = csc.cpStart;
                                return;
                            }

                            if (result > 0)
                            {
                                // replace the current code point with its full case mapping result
                                // see UCaseProperties.MaxStringLength
                                if (result <= UCaseProperties.MaxStringLength)
                                {
                                    /* replace by the mapping string */
                                    delta = result - UTF16.GetCharCount(c);
                                    text.Replace(csc.cpStart, textPos - csc.cpStart, replacementChars.AsSpan()); // ICU4N: Corrected 2nd parameter
                                    replacementChars.Length = 0;
                                }
                                else
                                {
                                    /* replace by single-code point mapping */
                                    ReadOnlySpan<char> utf32 = UTF16.ValueOf(result, codePointBuffer);
                                    delta = utf32.Length - UTF16.GetCharCount(c);
                                    text.Replace(csc.cpStart, textPos - csc.cpStart, utf32); // ICU4N: Corrected 2nd parameter
                                }

                                if (delta != 0)
                                {
                                    textPos += delta;
                                    csc.limit = offsets.ContextLimit += delta;
                                    offsets.Limit += delta;
                                }
                            }
                        }
                    }
                    offsets.Start = offsets.Limit;
                }
                finally
                {
                    handle.Free(); // Release the handle so the GC can collect the IReplaceable instance
                }
            }
        }

        // NOTE: normally this would be static, but because the results vary by locale....
        SourceTargetUtility sourceTargetUtility = null;

        /// <seealso cref="Transliterator.AddSourceTargetSet(UnicodeSet, UnicodeSet, UnicodeSet)"/>
#pragma warning disable 672
        public override void AddSourceTargetSet(UnicodeSet inputFilter, UnicodeSet sourceSet, UnicodeSet targetSet)
#pragma warning restore 672
        {
            if (sourceTargetUtility == null)
            {
                LazyInitializer.EnsureInitialized(ref sourceTargetUtility, () =>
                {
                    return new SourceTargetUtility(new ToTitleCaseTransform(locale));
                });
            }
            sourceTargetUtility.AddSourceTargetSet(this, inputFilter, sourceSet, targetSet);
        }

        private sealed class ToTitleCaseTransform : IStringTransform
        {
            private readonly UCultureInfo locale;
            public ToTitleCaseTransform(UCultureInfo locale)
            {
                this.locale = locale ?? throw new ArgumentNullException(nameof(locale));
            }

            public string Transform(string source)
                => UChar.ToTitleCase(locale, source, null);
        }
    }
}
