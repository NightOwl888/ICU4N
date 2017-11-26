using ICU4N.Impl;
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
            // Unicode: "U+10FFFF" hex, min=4, max=6
            Transliterator.RegisterFactory("Any-Hex/Unicode", new Factory(getInstance: (id) =>
            {
                return new EscapeTransliterator("Any-Hex/Unicode",
                                                "U+", "", 16, 4, true, null);
            }));

            // Java: "\\uFFFF" hex, min=4, max=4
            Transliterator.RegisterFactory("Any-Hex/DotNet", new Factory(getInstance: (id) =>
            {
                return new EscapeTransliterator("Any-Hex/DotNet",
                                                "\\u", "", 16, 4, false, null);
            }));

            // C: "\\uFFFF" hex, min=4, max=4; \\U0010FFFF hex, min=8, max=8
            Transliterator.RegisterFactory("Any-Hex/C", new Factory(getInstance: (id) =>
            {
                return new EscapeTransliterator("Any-Hex/C",
                                            "\\u", "", 16, 4, true,
                   new EscapeTransliterator("", "\\U", "", 16, 8, true, null));
            }));

            // XML: "&#x10FFFF;" hex, min=1, max=6
            Transliterator.RegisterFactory("Any-Hex/XML", new Transliterator.Factory(getInstance: (id) =>
            {
                return new EscapeTransliterator("Any-Hex/XML",
                                        "&#x", ";", 16, 1, true, null);
            }));

            // XML10: "&1114111;" dec, min=1, max=7 (not really "Any-Hex")
            Transliterator.RegisterFactory("Any-Hex/XML10", new Transliterator.Factory(getInstance: (id) =>
            {
                return new EscapeTransliterator("Any-Hex/XML10",
                                        "&#", ";", 10, 1, true, null);
            }));

            // Perl: "\\x{263A}" hex, min=1, max=6
            Transliterator.RegisterFactory("Any-Hex/Perl", new Transliterator.Factory(getInstance: (id) =>
            {
                return new EscapeTransliterator("Any-Hex/Perl",
                                        "\\x{", "}", 16, 1, true, null);
            }));

            // Plain: "FFFF" hex, min=4, max=6
            Transliterator.RegisterFactory("Any-Hex/Plain", new Transliterator.Factory(getInstance: (id) =>
            {
                return new EscapeTransliterator("Any-Hex/Plain",
                                        "", "", 16, 4, true, null);
            }));

            // Generic
            Transliterator.RegisterFactory("Any-Hex", new Transliterator.Factory(getInstance: (id) =>
            {
                return new EscapeTransliterator("Any-Hex",
                                        "\\u", "", 16, 4, false, null);
            }));
        }

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
        /// Implements <see cref="Transliterator.HandleTransliterate(IReplaceable, Position, bool)"/>.
        /// </summary>
        protected override void HandleTransliterate(IReplaceable text,
                                           Position pos, bool incremental)
        {
            int start = pos.Start;
            int limit = pos.Limit;

            StringBuilder buf = new StringBuilder(prefix);
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
                    Utility.AppendNumber(buf, c, supplementalHandler.radix,
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
                    Utility.AppendNumber(buf, c, radix, minDigits);
                    buf.Append(suffix);
                }

                text.Replace(start, start + charLen, buf.ToString());
                start += buf.Length;
                limit += buf.Length - charLen;
            }

            pos.ContextLimit += limit - pos.Limit;
            pos.Limit = limit;
            pos.Start = start;
        }

        /// <seealso cref="Transliterator.AddSourceTargetSet(UnicodeSet, UnicodeSet, UnicodeSet)"/>
        public override void AddSourceTargetSet(UnicodeSet inputFilter, UnicodeSet sourceSet, UnicodeSet targetSet)
        {
            sourceSet.AddAll(GetFilterAsUnicodeSet(inputFilter));
            for (EscapeTransliterator it = this; it != null; it = it.supplementalHandler)
            {
                if (inputFilter.Count != 0)
                {
                    targetSet.AddAll(it.prefix);
                    targetSet.AddAll(it.suffix);
                    StringBuilder buffer = new StringBuilder();
                    for (int i = 0; i < it.radix; ++i)
                    {
                        Utility.AppendNumber(buffer, i, it.radix, it.minDigits);
                    }
                    targetSet.AddAll(buffer.ToString()); // TODO drop once String is changed to CharSequence in UnicodeSet
                }
            }
        }
    }
}
