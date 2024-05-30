using ICU4N.Impl;
using ICU4N.Support.Text;
using J2N.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace ICU4N.Text
{
    /// <summary>
    /// Implemenation class for lower-/upper-/title-casing transliterators.
    /// Ported to C# from casetrn.cpp.
    /// </summary>
    // ICU4N: This was ported from C++ so we can factor out ReplaceableContextIterator and the ContextIterator interface.
    // We needed to eliminate these so upper/lower case can support ReadOnlySpan<char>.
    internal abstract class CaseMapTransliterator : Transliterator
    {
        protected object syncLock = new object();

        protected CaseMapTransliterator(string id, UCaseMapFull fMap, CaseLocale caseLocale)
            : base(id, null)
        {
            this.fMap = fMap ?? throw new ArgumentNullException(nameof(fMap));
            this.caseLocale = caseLocale;
        }

        /// <summary>
        /// Implements <see cref="Transliterator.HandleTransliterate(IReplaceable, TransliterationPosition, bool)"/>.
        /// </summary>
        /// <param name="text">         The buffer holding transliterated and
        ///                             untransliterated text.</param>
        /// <param name="offsets">      The start and limit of the text, the position
        ///                             of the cursor, and the start and limit of transliteration.</param>
        /// <param name="isIncremental">If true, assume more text may be coming after
        ///                             <see cref="TransliterationPosition.ContextLimit"/>.  Otherwise, assume the text is complete.</param>
        protected override void HandleTransliterate(IReplaceable text, TransliterationPosition offsets, bool isIncremental)
        {
            lock (syncLock)
            {
                if (offsets.Start >= offsets.Limit)
                {
                    return;
                }

                UCaseContext csc = new UCaseContext();
                GCHandle handle = GCHandle.Alloc(text, GCHandleType.Normal);
                try
                {
                    csc.p = GCHandle.ToIntPtr(handle);
                    csc.start = offsets.ContextStart;
                    csc.limit = offsets.ContextLimit;

                    ValueStringBuilder replacementChars = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);

                    int c;
                    int textPos, delta = 0, result;

                    for (textPos = offsets.Start; textPos < offsets.Limit;)
                    {
                        csc.cpStart = textPos;
                        c = text.Char32At(textPos);
                        csc.cpLimit = textPos += UTF16.GetCharCount(c);

                        unsafe
                        {
                            result = fMap(c, Replaceable.CaseContextIterator, (IntPtr)(&csc), ref replacementChars, caseLocale);
                        }

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
                                ReadOnlySpan<char> utf32 = UTF16.ValueOf(result, stackalloc char[2]);
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
                    offsets.Start = textPos;
                }
                finally
                {
                    handle.Free(); // Release the handle so the GC can collect the IReplaceable instance
                }
            }
        }

        private readonly UCaseMapFull fMap;
        private readonly CaseLocale caseLocale;
    }
}
