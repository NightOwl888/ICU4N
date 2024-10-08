﻿using ICU4N.Impl;
using System.Text;

namespace ICU4N.Text
{
    /// <summary>
    /// A transliterator that converts Unicode characters to an escape
    /// form.  Examples of escape forms are "U+4E01" and "&#x10FFFF;".
    /// </summary>
    /// <remarks>
    /// Escape forms have a prefix and suffix, either of which may be
    /// empty, a radix, typically 16 or 10, a minimum digit count,
    /// typically 1, 4, or 8, and a boolean that specifies whether
    /// supplemental characters are handled as 32-bit code points or as two
    /// 16-bit code units.  Most escape forms handle 32-bit code points,
    /// but some, such as the Java form, intentionally break them into two
    /// surrogate pairs, for backward compatibility.
    /// <para/>
    /// Some escape forms actually have two different patterns, one for
    /// BMP characters (0..FFFF) and one for supplements (>FFFF).  To
    /// handle this, a second EscapeTransliterator may be defined that
    /// specifies the pattern to be produced for supplementals.  An example
    /// of a form that requires this is the C form, which uses "\\uFFFF"
    /// for BMP characters and "\\U0010FFFF" for supplementals.
    /// <para/>
    /// This class is package private.  It registers several standard
    /// variants with the system which are then accessed via their IDs.
    /// </remarks>
    /// <author>Alan Liu</author>
    internal class EscapeTransliterator : Transliterator
    {
        /// <summary>
        /// The prefix of the escape form; may be empty, but usually isn't.
        /// May not be null.
        /// </summary>
        private string prefix;

        /// <summary>
        /// The prefix of the escape form; often empty.  May not be null.
        /// </summary>
        private string suffix;

        /// <summary>
        /// The radix to display the number in.  Typically 16 or 10.  Must
        /// be in the range 2 to 36.
        /// </summary>
        private int radix;

        /// <summary>
        /// The minimum number of digits.  Typically 1, 4, or 8.  Values
        /// less than 1 are equivalent to 1.
        /// </summary>
        private int minDigits;

        /// <summary>
        /// If true, supplementals are handled as 32-bit code points.  If
        /// false, they are handled as two 16-bit code units.
        /// </summary>
        private bool grokSupplementals;

        /// <summary>
        /// The form to be used for supplementals.  If this is null then
        /// the same form is used for BMP characters and supplementals.  If
        /// this is not null and if grokSupplementals is true then the
        /// prefix, suffix, radix, and minDigits of this object are used
        /// for supplementals.
        /// </summary>
        private EscapeTransliterator supplementalHandler;

        /// <summary>
        /// Registers standard variants with the system.  Called by
        /// Transliterator during initialization.
        /// </summary>
        internal static void Register()
        {
            UnicodeTransliteratorFactory.Register();
            JavaTransliteratorFactory.Register();
            DotNetTransliteratorFactory.Register();
            CTransliteratorFactory.Register();
            XmlTransliteratorFactory.Register();
            Xml10TransliteratorFactory.Register();
            PerlTransliteratorFactory.Register();
            PlainTransliteratorFactory.Register();
            GenericTransliteratorFactory.Register();
        }

        #region Factories
        // ICU4N: These were anonymous classes in Java. The closest equivalent in .NET
        // (a generic Factory class that accepts a Func<T>) performs poorly.
        // So, we use explicitly delcared classes instead. Moving the Register()
        // part into the class so it is all in context.

        // Unicode: "U+10FFFF" hex, min=4, max=6
        private sealed class UnicodeTransliteratorFactory : ITransliteratorFactory
        {
            public static void Register()
                => RegisterFactory("Any-Hex/Unicode", new UnicodeTransliteratorFactory());

            public Transliterator GetInstance(string id)
                => new EscapeTransliterator("Any-Hex/Unicode",
                                            "U+", "", 16, 4, true, null);
        }

        // Java: "\\uFFFF" hex, min=4, max=4
        private sealed class JavaTransliteratorFactory : ITransliteratorFactory
        {
            public static void Register()
                => RegisterFactory("Any-Hex/Java", new JavaTransliteratorFactory());

            public Transliterator GetInstance(string id)
                => new EscapeTransliterator("Any-Hex/Java",
                                            "\\u", "", 16, 4, false, null);
        }

        // .NET: "\\uFFFF" hex, min=4, max=4
        private sealed class DotNetTransliteratorFactory : ITransliteratorFactory
        {
            public static void Register()
                => RegisterFactory("Any-Hex/DotNet", new DotNetTransliteratorFactory());

            public Transliterator GetInstance(string id)
                => new EscapeTransliterator("Any-Hex/DotNet",
                                            "\\u", "", 16, 4, false, null);
        }

        // C: "\\uFFFF" hex, min=4, max=4; \\U0010FFFF hex, min=8, max=8
        private sealed class CTransliteratorFactory : ITransliteratorFactory
        {
            public static void Register()
                => RegisterFactory("Any-Hex/C", new CTransliteratorFactory());

            public Transliterator GetInstance(string id)
            {
                return new EscapeTransliterator("Any-Hex/C",
                                            "\\u", "", 16, 4, true,
                   new EscapeTransliterator("", "\\U", "", 16, 8, true, null));
            }
        }

        // XML: "&#x10FFFF;" hex, min=1, max=6
        private sealed class XmlTransliteratorFactory : ITransliteratorFactory
        {
            public static void Register()
                => RegisterFactory("Any-Hex/XML", new XmlTransliteratorFactory());

            public Transliterator GetInstance(string id)
                => new EscapeTransliterator("Any-Hex/XML",
                                            "&#x", ";", 16, 1, true, null);
        }

        // XML10: "&1114111;" dec, min=1, max=7 (not really "Any-Hex")
        private sealed class Xml10TransliteratorFactory : ITransliteratorFactory
        {
            public static void Register()
                => RegisterFactory("Any-Hex/XML10", new Xml10TransliteratorFactory());

            public Transliterator GetInstance(string id)
                => new EscapeTransliterator("Any-Hex/XML10",
                                            "&#", ";", 10, 1, true, null);
        }

        // Perl: "\\x{263A}" hex, min=1, max=6
        private sealed class PerlTransliteratorFactory : ITransliteratorFactory
        {
            public static void Register()
                => RegisterFactory("Any-Hex/Perl", new PerlTransliteratorFactory());

            public Transliterator GetInstance(string id)
                => new EscapeTransliterator("Any-Hex/Perl",
                                            "\\x{", "}", 16, 1, true, null);
        }

        // Plain: "FFFF" hex, min=4, max=6
        private sealed class PlainTransliteratorFactory : ITransliteratorFactory
        {
            public static void Register()
                => RegisterFactory("Any-Hex/Plain", new PlainTransliteratorFactory());

            public Transliterator GetInstance(string id)
                => new EscapeTransliterator("Any-Hex/Plain",
                                            "", "", 16, 4, true, null);
        }

        // Generic
        private sealed class GenericTransliteratorFactory : ITransliteratorFactory
        {
            public static void Register()
                => RegisterFactory("Any-Hex", new GenericTransliteratorFactory());

            public Transliterator GetInstance(string id)
                => new EscapeTransliterator("Any-Hex",
                                            "\\u", "", 16, 4, false, null);
        }

        #endregion Factories

        /// <summary>
        /// Constructs an escape transliterator with the given <paramref name="id"/> and
        /// parameters.  See the class member documentation for details.
        /// </summary>
        internal EscapeTransliterator(string id, string prefix, string suffix,
                             int radix, int minDigits,
                             bool grokSupplementals,
                             EscapeTransliterator supplementalHandler)
                : base(id, null)
        {
            this.prefix = prefix;
            this.suffix = suffix;
            this.radix = radix;
            this.minDigits = minDigits;
            this.grokSupplementals = grokSupplementals;
            this.supplementalHandler = supplementalHandler;
        }

        /// <summary>
        /// Implements <see cref="Transliterator.HandleTransliterate(IReplaceable, TransliterationPosition, bool)"/>.
        /// </summary>
        protected override void HandleTransliterate(IReplaceable text,
                                           TransliterationPosition pos, bool incremental)
        {
            int start = pos.Start;
            int limit = pos.Limit;

            ValueStringBuilder buf = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                buf.Append(prefix);
                int prefixLen = prefix.Length;
                bool redoPrefix = false;

                while (start < limit)
                {
                    int c = grokSupplementals ? text.Char32At(start) : text[start];
                    int charLen = grokSupplementals ? UTF16.GetCharCount(c) : 1;

                    if ((c & 0xFFFF0000) != 0 && supplementalHandler != null)
                    {
                        buf.Length = 0;
                        buf.Append(supplementalHandler.prefix);
                        Utility.AppendNumber(ref buf, c, supplementalHandler.radix,
                                             supplementalHandler.minDigits);
                        buf.Append(supplementalHandler.suffix);
                        redoPrefix = true;
                    }
                    else
                    {
                        if (redoPrefix)
                        {
                            buf.Length = 0;
                            buf.Append(prefix);
                            redoPrefix = false;
                        }
                        else
                        {
                            buf.Length = prefixLen;
                        }
                        Utility.AppendNumber(ref buf, c, radix, minDigits);
                        buf.Append(suffix);
                    }

                    text.Replace(start, charLen, buf.AsSpan()); // ICU4N: Corrected 2nd parameter
                    start += buf.Length;
                    limit += buf.Length - charLen;
                }

                pos.ContextLimit += limit - pos.Limit;
                pos.Limit = limit;
                pos.Start = start;
            }
            finally
            {
                buf.Dispose();
            }
        }

        /// <seealso cref="Transliterator.AddSourceTargetSet(UnicodeSet, UnicodeSet, UnicodeSet)"/>
#pragma warning disable 672
        public override void AddSourceTargetSet(UnicodeSet inputFilter, UnicodeSet sourceSet, UnicodeSet targetSet)
        {
#pragma warning disable 672
#pragma warning disable 612, 618
            sourceSet.AddAll(GetFilterAsUnicodeSet(inputFilter));
#pragma warning restore 612, 618
            if (inputFilter.Count != 0) // ICU4N: Moved this condition outside of the loop, since it requires some calculation
            {
                ValueStringBuilder buffer = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
                try
                {
                    for (EscapeTransliterator it = this; it != null; it = it.supplementalHandler)
                    {
                        targetSet.AddAll(it.prefix);
                        targetSet.AddAll(it.suffix);
                        buffer.Length = 0;
                        for (int i = 0; i < it.radix; ++i)
                        {
                            Utility.AppendNumber(ref buffer, i, it.radix, it.minDigits);
                        }
                        targetSet.AddAll(buffer.AsSpan()); // TODO drop once String is changed to CharSequence in UnicodeSet
                    }
                }
                finally
                {
                    buffer.Dispose();
                }
            }
        }
    }
}
