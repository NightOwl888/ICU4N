using ICU4N.Impl;
using ICU4N.Globalization;
using System.Text;

namespace ICU4N.Text
{
    /// <summary>
    /// A transliterator that converts Unicode escape forms to the
    /// characters they represent.  Escape forms have a prefix, a suffix, a
    /// radix, and minimum and maximum digit counts.
    /// <para/>
    /// This class is internal. It registers several standard
    /// variants with the system which are then accessed via their IDs.
    /// </summary>
    /// <author>Alan Liu</author>
    internal class UnescapeTransliterator : Transliterator
    {
        /// <summary>
        /// The encoded pattern specification.  The pattern consists of
        /// zero or more forms.  Each form consists of a prefix, suffix,
        /// radix, minimum digit count, and maximum digit count.  These
        /// values are stored as a five character header.  That is, their
        /// numeric values are cast to 16-bit characters and stored in the
        /// string.  Following these five characters, the prefix
        /// characters, then suffix characters are stored.  Each form thus
        /// takes n+5 characters, where n is the total length of the prefix
        /// and suffix.  The end is marked by a header of length one
        /// consisting of the character <see cref="END"/>.
        /// </summary>
        private char[] spec;

        /// <summary>
        /// Special character marking the end of the spec[] array.
        /// </summary>
        private const char END = (char)0xFFFF;

        /// <summary>
        /// Registers standard variants with the system.  Called by
        /// <see cref="Transliterator"/> during initialization.
        /// </summary>
        internal static void Register()
        {
            // Unicode: "U+10FFFF" hex, min=4, max=6
            Transliterator.RegisterFactory("Hex-Any/Unicode", new Transliterator.Factory(getInstance: (id) =>
            {
                return new UnescapeTransliterator("Hex-Any/Unicode", new char[] {
                    (char)2, (char)0, (char)16, (char)4, (char)6, 'U', '+',
                    END
                });
            }));

            // Java: "\\uFFFF" hex, min=4, max=4
            Transliterator.RegisterFactory("Hex-Any/DotNet", new Transliterator.Factory(getInstance: (id) =>
            {
                return new UnescapeTransliterator("Hex-Any/DotNet", new char[] {
                    (char)2, (char)0, (char)16, (char)4, (char)4, '\\', 'u',
                    END
                });
            }));

            // C: "\\uFFFF" hex, min=4, max=4; \\U0010FFFF hex, min=8, max=8
            Transliterator.RegisterFactory("Hex-Any/C", new Transliterator.Factory(getInstance: (id) =>
            {
                return new UnescapeTransliterator("Hex-Any/C", new char[] {
                    (char)2, (char)0, (char)16, (char)4, (char)4, '\\', 'u',
                    (char)2, (char)0, (char)16, (char)8, (char)8, '\\', 'U',
                    END
                });
            }));

            // XML: "&#x10FFFF;" hex, min=1, max=6
            Transliterator.RegisterFactory("Hex-Any/XML", new Transliterator.Factory(getInstance: (id) =>
            {
                return new UnescapeTransliterator("Hex-Any/XML", new char[] {
                    (char)3, (char)1, (char)16, (char)1, (char)6, '&', '#', 'x', ';',
                    END
                });
            }));

            // XML10: "&1114111;" dec, min=1, max=7 (not really "Hex-Any")
            Transliterator.RegisterFactory("Hex-Any/XML10", new Transliterator.Factory(getInstance: (id) =>
            {
                return new UnescapeTransliterator("Hex-Any/XML10", new char[] {
                    (char)2, (char)1, (char)10, (char)1, (char)7, '&', '#', ';',
                    END
                });
            }));

            // Perl: "\\x{263A}" hex, min=1, max=6
            Transliterator.RegisterFactory("Hex-Any/Perl", new Transliterator.Factory(getInstance: (id) =>
            {
                return new UnescapeTransliterator("Hex-Any/Perl", new char[] {
                    (char)3, (char)1, (char)16, (char)1, (char)6, '\\', 'x', '{', '}',
                    END
                });
            }));

            // All: Java, C, Perl, XML, XML10, Unicode
            Transliterator.RegisterFactory("Hex-Any", new Transliterator.Factory(getInstance: (id) =>
            {
                return new UnescapeTransliterator("Hex-Any", new char[] {
                    (char)2,(char) 0, (char)16, (char)4, (char)6, 'U', '+',            // Unicode
                    (char)2, (char)0, (char)16, (char)4, (char)4, '\\', 'u',           // Java
                    (char)2, (char)0, (char)16, (char)8, (char)8, '\\', 'U',           // C (surrogates)
                    (char)3, (char)1, (char)16, (char)1, (char)6, '&', '#', 'x', ';',  // XML
                    (char)2, (char)1, (char)10, (char)1, (char)7, '&', '#', ';',       // XML10
                    (char)3, (char)1, (char)16, (char)1, (char)6, '\\', 'x', '{', '}', // Perl
                    END
                });
            }));
        }

        /// <summary>
        /// Internal constructor.  Takes the encoded spec array.
        /// </summary>
        internal UnescapeTransliterator(string id, char[] spec)
                : base(id, null)
        {
            this.spec = spec;
        }

        /// <summary>
        /// Implements <see cref="Transliterator.HandleTransliterate(IReplaceable, TransliterationPosition, bool)"/>.
        /// </summary>
        protected override void HandleTransliterate(IReplaceable text,
                                           TransliterationPosition pos, bool isIncremental)
        {
            int start = pos.Start;
            int limit = pos.Limit;
            int i, ipat;

            //loop:
            while (start < limit)
            {
                // Loop over the forms in spec[].  Exit this loop when we
                // match one of the specs.  Exit the outer loop if a
                // partial match is detected and isIncremental is true.
                for (ipat = 0; spec[ipat] != END;)
                {

                    // Read the header
                    int prefixLen = spec[ipat++];
                    int suffixLen = spec[ipat++];
                    int radix = spec[ipat++];
                    int minDigits = spec[ipat++];
                    int maxDigits = spec[ipat++];

                    // s is a copy of start that is advanced over the
                    // characters as we parse them.
                    int s = start;
                    bool match = true;

                    for (i = 0; i < prefixLen; ++i)
                    {
                        if (s >= limit)
                        {
                            if (i > 0)
                            {
                                // We've already matched a character.  This is
                                // a partial match, so we return if in
                                // incremental mode.  In non-incremental mode,
                                // go to the next spec.
                                if (isIncremental)
                                {
                                    goto loop_break;
                                }
                                match = false;
                                break;
                            }
                        }
                        char c = text[s++];
                        if (c != spec[ipat + i])
                        {
                            match = false;
                            break;
                        }
                    }

                    if (match)
                    {
                        int u = 0;
                        int digitCount = 0;
                        for (; ; )
                        {
                            if (s >= limit)
                            {
                                // Check for partial match in incremental mode.
                                if (s > start && isIncremental)
                                {
                                    goto loop_break;
                                }
                                break;
                            }
                            int ch = text.Char32At(s);
                            int digit = UChar.Digit(ch, radix);
                            if (digit < 0)
                            {
                                break;
                            }
                            s += UTF16.GetCharCount(ch);
                            u = (u * radix) + digit;
                            if (++digitCount == maxDigits)
                            {
                                break;
                            }
                        }

                        match = (digitCount >= minDigits);

                        if (match)
                        {
                            for (i = 0; i < suffixLen; ++i)
                            {
                                if (s >= limit)
                                {
                                    // Check for partial match in incremental mode.
                                    if (s > start && isIncremental)
                                    {
                                        goto loop_break;
                                    }
                                    match = false;
                                    break;
                                }
                                char c = text[s++];
                                if (c != spec[ipat + prefixLen + i])
                                {
                                    match = false;
                                    break;
                                }
                            }

                            if (match)
                            {
                                // At this point, we have a match
                                string str = UTF16.ValueOf(u);
                                text.Replace(start, s - start, str); // ICU4N: Corrected 2nd parameter
                                limit -= s - start - str.Length;
                                // The following break statement leaves the
                                // loop that is traversing the forms in
                                // spec[].  We then parse the next input
                                // character.
                                break;
                            }
                        }
                    }

                    ipat += prefixLen + suffixLen;
                }

                if (start < limit)
                {
                    start += UTF16.GetCharCount(text.Char32At(start));
                }
            }
            loop_break: { }

            pos.ContextLimit += limit - pos.Limit;
            pos.Limit = limit;
            pos.Start = start;
        }

        /// <seealso cref="Transliterator.AddSourceTargetSet(UnicodeSet, UnicodeSet, UnicodeSet)"/>
#pragma warning disable 672
        public override void AddSourceTargetSet(UnicodeSet inputFilter, UnicodeSet sourceSet, UnicodeSet targetSet)
#pragma warning restore 672
        {
            // Each form consists of a prefix, suffix,
            // * radix, minimum digit count, and maximum digit count.  These
            // * values are stored as a five character header. ...
#pragma warning disable 612, 618
            UnicodeSet myFilter = GetFilterAsUnicodeSet(inputFilter);
#pragma warning restore 612, 618
            UnicodeSet items = new UnicodeSet();
            StringBuilder buffer = new StringBuilder();
            for (int i = 0; spec[i] != END;)
            {
                // first 5 items are header
                int end = i + spec[i] + spec[i + 1] + 5;
                int radix = spec[i + 2];
                for (int j = 0; j < radix; ++j)
                {
                    Utility.AppendNumber(buffer, j, radix, 0);
                }
                // then add the characters
                for (int j = i + 5; j < end; ++j)
                {
                    items.Add(spec[j]);
                }
                // and go to next block
                i = end;
            }
            items.AddAll(buffer.ToString());
            items.RetainAll(myFilter);

            if (items.Count > 0)
            {
                sourceSet.AddAll(items);
                targetSet.AddAll(0, 0x10FFFF); // assume we can produce any character
            }
        }
    }
}
