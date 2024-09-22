using ICU4N.Impl;
using ICU4N.Text;
using System;
using System.CodeDom;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace ICU4N.Text
{
    /// <summary>
    /// A transliterator that performs locale-sensitive ToLower()
    /// case mapping.
    /// </summary>
    internal class CaseFoldTransliterator : Transliterator
    {
        private readonly object syncLock = new object();

        /// <summary>
        /// Package accessible ID.
        /// </summary>
        internal const string _ID = "Any-CaseFold";

        // TODO: Add variants for tr, az, lt, default = default locale

        /// <summary>
        /// System registration hook.
        /// </summary>
        internal static void Register()
        {
            Transliterator.RegisterFactory(_ID, new CaseFoldTransliteratorFactory());

            Transliterator.RegisterSpecialInverse("CaseFold", "Upper", false);
        }

        private sealed class CaseFoldTransliteratorFactory : ITransliteratorFactory
        {
            public Transliterator GetInstance(string id)
                => new CaseFoldTransliterator();
        }

        private readonly UCaseProperties csp;

        /// <summary>
        /// Constructs a transliterator.
        /// </summary>
        public CaseFoldTransliterator()
                : base(_ID, null)
        {
            csp = UCaseProperties.Instance;
        }

        /// <summary>
        /// Implements <see cref="Transliterator.HandleTransliterate(IReplaceable, TransliterationPosition, bool)"/>
        /// </summary>
        // ICU4N: Couldn't locate the equivalent functionality in C++, so this is mostly a copy of CaseMapTransliterator.HandleTransliterate.
        // ToFullFolding doesn't match the delegate signature used in CaseMapTransliterator, so it cannot be utilized directly, but
        // the only difference is that method call.
        protected override void HandleTransliterate(IReplaceable text,
                                           TransliterationPosition offsets, bool isIncremental)
        {
            lock (syncLock)
            {
                if (csp == null)
                {
                    return;
                }

                if (offsets.Start >= offsets.Limit)
                {
                    return;
                }

                ValueStringBuilder replacementChars = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
                UCaseContext csc = new UCaseContext();
                GCHandle handle = GCHandle.Alloc(text, GCHandleType.Normal);
                try
                {
                    csc.p = GCHandle.ToIntPtr(handle);
                    csc.start = offsets.ContextStart;
                    csc.limit = offsets.ContextLimit;

                    int c;
                    int textPos, delta = 0, result;

                    Span<char> codePointBuffer = stackalloc char[2];

                    // Walk through original string
                    // If there is a case change, modify corresponding position in replaceable
                    for (textPos = offsets.Start; textPos < offsets.Limit;)
                    {
                        csc.cpStart = textPos;
                        c = text.Char32At(textPos);
                        csc.cpLimit = textPos += UTF16.GetCharCount(c);

                        result = csp.ToFullFolding(c, ref replacementChars, 0); // toFullFolding(int c, StringBuffer out, int options)

                        if (csc.b1 && isIncremental)
                        {
                            // the case mapping function tried to look beyond the context limit
                            // wait for more input
                            offsets.Start = csc.cpStart;
                            return;
                        }

                        /* decode the result */
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
                    offsets.Start = offsets.Limit;
                }
                finally
                {
                    handle.Free(); // Release the handle so the GC can collect the IReplaceable instance
                    replacementChars.Dispose();
                }
            }
        }

        internal static SourceTargetUtility sourceTargetUtility = null;

        /// <seealso cref="Transliterator.AddSourceTargetSet(UnicodeSet, UnicodeSet, UnicodeSet)"/>
#pragma warning disable 672
        public override void AddSourceTargetSet(UnicodeSet inputFilter, UnicodeSet sourceSet, UnicodeSet targetSet)
#pragma warning restore 672
        {
            if (sourceTargetUtility == null)
            {
                LazyInitializer.EnsureInitialized(ref sourceTargetUtility, () =>
                {
                    return new SourceTargetUtility(new CaseFoldTransform());
                });
            }

            sourceTargetUtility.AddSourceTargetSet(this, inputFilter, sourceSet, targetSet);
        }

        private sealed class CaseFoldTransform : IStringTransform
        {
            public string Transform(string source)
                => UChar.FoldCase(source, true);
        }
    }
}
