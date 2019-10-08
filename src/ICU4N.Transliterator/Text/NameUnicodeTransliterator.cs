using ICU4N.Impl;
using ICU4N.Globalization;
using System;
using System.Collections.Generic;
using System.Text;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Text
{
    /// <summary>
    /// A transliterator that performs name to character mapping.
    /// </summary>
    /// <author>Alan Liu</author>
    internal class NameUnicodeTransliterator : Transliterator
    {
        internal const string _ID = "Name-Any";

        internal const string OPEN_PAT = "\\N~{~";
        internal const char OPEN_DELIM = '\\'; // first char of OPEN_PAT
        internal const char CLOSE_DELIM = '}';
        internal const char SPACE = ' ';


        /// <summary>
        /// System registration hook.
        /// </summary>
        internal static void Register()
        {
            Transliterator.RegisterFactory(_ID, new Transliterator.Factory(getInstance: (id) =>
            {
                return new NameUnicodeTransliterator(null);
            }));
        }

        /// <summary>
        /// Constructs a transliterator.
        /// </summary>
        /// <param name="filter"></param>
        public NameUnicodeTransliterator(UnicodeFilter filter)
                : base(_ID, filter)
        {
        }

        /// <summary>
        /// Implements <see cref="Transliterator.HandleTransliterate(IReplaceable, TransliterationPosition, bool)"/>.
        /// </summary>
        protected override void HandleTransliterate(IReplaceable text,
                                           TransliterationPosition offsets, bool isIncremental)
        {

            int maxLen = UCharacterName.Instance.MaxCharNameLength + 1; // allow for temporary trailing space

            StringBuffer name = new StringBuffer(maxLen);

            // Get the legal character set
            UnicodeSet legal = new UnicodeSet();
            UCharacterName.Instance.GetCharNameCharacters(legal);

            int cursor = offsets.Start;
            int limit = offsets.Limit;

            // Modes:
            // 0 - looking for open delimiter
            // 1 - after open delimiter
            int mode = 0;
            int openPos = -1; // open delim candidate pos

            int c;
            while (cursor < limit)
            {
                c = text.Char32At(cursor);

                switch (mode)
                {
                    case 0: // looking for open delimiter
                        if (c == OPEN_DELIM)
                        { // quick check first
                            openPos = cursor;
                            int i = Utility.ParsePattern(OPEN_PAT, text, cursor, limit);
                            if (i >= 0 && i < limit)
                            {
                                mode = 1;
                                name.Length = 0;
                                cursor = i;
                                continue; // *** reprocess char32At(cursor)
                            }
                        }
                        break;

                    case 1: // after open delimiter
                            // Look for legal chars.  If \s+ is found, convert it
                            // to a single space.  If closeDelimiter is found, exit
                            // the loop.  If any other character is found, exit the
                            // loop.  If the limit is reached, exit the loop.

                        // Convert \s+ => SPACE.  This assumes there are no
                        // runs of >1 space characters in names.
                        if (PatternProps.IsWhiteSpace(c))
                        {
                            // Ignore leading whitespace
                            if (name.Length > 0 &&
                                name[name.Length - 1] != SPACE)
                            {
                                name.Append(SPACE);
                                // If we are too long then abort.  maxLen includes
                                // temporary trailing space, so use '>'.
                                if (name.Length > maxLen)
                                {
                                    mode = 0;
                                }
                            }
                            break;
                        }

                        if (c == CLOSE_DELIM)
                        {

                            int len = name.Length;

                            // Delete trailing space, if any
                            if (len > 0 &&
                                name[len - 1] == SPACE)
                            {
                                name.Length = --len;
                            }

                            c = UChar.GetCharFromExtendedName(name.ToString());
                            if (c != -1)
                            {
                                // Lookup succeeded

                                // assert(UTF16.getCharCount(CLOSE_DELIM) == 1);
                                cursor++; // advance over CLOSE_DELIM

                                string str = UTF16.ValueOf(c);
                                text.Replace(openPos, cursor, str);

                                // Adjust indices for the change in the length of
                                // the string.  Do not assume that str.length() ==
                                // 1, in case of surrogates.
                                int delta = cursor - openPos - str.Length;
                                cursor -= delta;
                                limit -= delta;
                                // assert(cursor == openPos + str.length());
                            }
                            // If the lookup failed, we leave things as-is and
                            // still switch to mode 0 and continue.
                            mode = 0;
                            openPos = -1; // close off candidate
                            continue; // *** reprocess char32At(cursor)
                        }

                        if (legal.Contains(c))
                        {
                            UTF16.Append(name, c);
                            // If we go past the longest possible name then abort.
                            // maxLen includes temporary trailing space, so use '>='.
                            if (name.Length >= maxLen)
                            {
                                mode = 0;
                            }
                        }

                        // Invalid character
                        else
                        {
                            --cursor; // Backup and reprocess this character
                            mode = 0;
                        }

                        break;
                }

                cursor += UTF16.GetCharCount(c);
            }

            offsets.ContextLimit += limit - offsets.Limit;
            offsets.Limit = limit;
            // In incremental mode, only advance the cursor up to the last
            // open delimiter candidate.
            offsets.Start = (isIncremental && openPos >= 0) ? openPos : cursor;
        }

        /// <seealso cref="Transliterator.AddSourceTargetSet(UnicodeSet, UnicodeSet, UnicodeSet)"/>
#pragma warning disable 672
        public override void AddSourceTargetSet(UnicodeSet inputFilter, UnicodeSet sourceSet, UnicodeSet targetSet)
        {
#pragma warning disable 672
#pragma warning disable 612, 618
            UnicodeSet myFilter = GetFilterAsUnicodeSet(inputFilter);
#pragma warning restore 612, 618
            if (!myFilter.ContainsAll(UnicodeNameTransliterator.OPEN_DELIM) || !myFilter.Contains(CLOSE_DELIM))
            {
                return; // we have to contain both prefix and suffix
            }
            UnicodeSet items = new UnicodeSet()
            .AddAll('0', '9')
            .AddAll('A', 'F')
            .AddAll('a', 'z') // for controls
            .Add('<').Add('>') // for controls
            .Add('(').Add(')') // for controls
            .Add('-')
            .Add(' ')
            .AddAll(UnicodeNameTransliterator.OPEN_DELIM)
            .Add(CLOSE_DELIM);
            items.RetainAll(myFilter);
            if (items.Count > 0)
            {
                sourceSet.AddAll(items);
                // could produce any character
                targetSet.AddAll(0, 0x10FFFF);
            }
        }
    }
}
