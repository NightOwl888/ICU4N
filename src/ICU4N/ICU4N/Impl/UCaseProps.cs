using ICU4N.Lang;
using ICU4N.Support.IO;
using ICU4N.Support.Text;
using ICU4N.Text;
using ICU4N.Util;
using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace ICU4N.Impl
{
    /// <summary>
    /// Low-level Unicode character/string case mapping code.
    /// .NET port of ucase.h/.c.
    /// </summary>
    /// <author>Markus W. Scherer</author>
    /// <created>2005jan29</created>
    public sealed partial class UCaseProps
    {
        // constructors etc. --------------------------------------------------- ***

        // port of ucase_openProps()
        private UCaseProps()
        {
            ByteBuffer bytes = ICUBinary.GetRequiredData(DATA_FILE_NAME);
            ReadData(bytes);
        }

        private void ReadData(ByteBuffer bytes)
        {
            // read the header
            ICUBinary.ReadHeader(bytes, FMT, new IsAcceptable());

            // read indexes[]
            int count = bytes.GetInt32();
            if (count < IX_TOP)
            {
                throw new IOException("indexes[0] too small in " + DATA_FILE_NAME);
            }
            indexes = new int[count];

            indexes[0] = count;
            for (int i = 1; i < count; ++i)
            {
                indexes[i] = bytes.GetInt32();
            }

            // read the trie
            trie = Trie2_16.CreateFromSerialized(bytes);
            int expectedTrieLength = indexes[IX_TRIE_SIZE];
            int trieLength = trie.GetSerializedLength();
            if (trieLength > expectedTrieLength)
            {
                throw new IOException(DATA_FILE_NAME + ": not enough bytes for the trie");
            }
            // skip padding after trie bytes
            ICUBinary.SkipBytes(bytes, expectedTrieLength - trieLength);

            // read exceptions[]
            count = indexes[IX_EXC_LENGTH];
            if (count > 0)
            {
                exceptions = ICUBinary.GetString(bytes, count, 0);
            }

            // read unfold[]
            count = indexes[IX_UNFOLD_LENGTH];
            if (count > 0)
            {
                unfold = ICUBinary.GetChars(bytes, count, 0);
            }
        }

        // implement IAuthenticate
        private sealed class IsAcceptable : IAuthenticate
        {
            public bool IsDataVersionAcceptable(byte[] version)
            {
                return version[0] == 3;
            }
        }

        // set of property starts for UnicodeSet ------------------------------- ***

        public void AddPropertyStarts(UnicodeSet set)
        {
            /* add the start code point of each same-value range of the trie */
            using (var trieIterator = trie.GetEnumerator())
            {
                Trie2.Range range;
                while (trieIterator.MoveNext() && !(range = trieIterator.Current).LeadSurrogate)
                {
                    set.Add(range.StartCodePoint);
                }
            }

            /* add code points with hardcoded properties, plus the ones following them */

            /* (none right now, see comment below) */

            /*
             * Omit code points with hardcoded specialcasing properties
             * because we do not build property UnicodeSets for them right now.
             */
        }

        // data access primitives ---------------------------------------------- ***
        private static int GetExceptionsOffset(int props)
        {
            return props >> EXC_SHIFT;
        }

        private static bool PropsHasException(int props)
        {
            return (props & EXCEPTION) != 0;
        }

        /// <summary>number of bits in an 8-bit integer value</summary>
        private static readonly byte[/*256*/] flagsOffset ={
            0, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4,
            1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5,
            1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5,
            2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,
            1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5,
            2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,
            2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,
            3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7,
            1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5,
            2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,
            2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,
            3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7,
            2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,
            3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7,
            3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7,
            4, 5, 5, 6, 5, 6, 6, 7, 5, 6, 6, 7, 6, 7, 7, 8
        };

        private static bool HasSlot(int flags, int index)
        {
            return (flags & (1 << index)) != 0;
        }
        private static byte SlotOffset(int flags, int index)
        {
            return flagsOffset[flags & ((1 << index) - 1)];
        }

        /// <summary>
        /// Get the value of an optional-value slot where HasSlot(excWord, index).
        /// </summary>
        /// <param name="excWord">Initial exceptions word.</param>
        /// <param name="index">Desired slot index.</param>
        /// <param name="excOffset">offset into exceptions[] after excWord=exceptions[excOffset++];</param>
        /// <returns>bits 31..0: slot value
        ///             63..32: modified excOffset, moved to the last char of the value, use +1 for beginning of next slot</returns>
        private long GetSlotValueAndOffset(int excWord, int index, int excOffset)
        {
            long value;
            if ((excWord & EXC_DOUBLE_SLOTS) == 0)
            {
                excOffset += SlotOffset(excWord, index);
                value = exceptions[excOffset];
            }
            else
            {
                excOffset += 2 * SlotOffset(excWord, index);
                value = exceptions[excOffset++];
                value = (value << 16) | exceptions[excOffset];
            }
            return value | ((long)excOffset << 32);
        }

        /// <summary>Same as <see cref="GetSlotValueAndOffset(int, int, int)"/> but does not return the slot offset.</summary>
        /// <param name="excWord">Initial exceptions word.</param>
        /// <param name="index">Desired slot index.</param>
        /// <param name="excOffset">offset into exceptions[] after excWord=exceptions[excOffset++];</param>
        /// <returns>bits 63..32: modified excOffset, moved to the last char of the value, use +1 for beginning of next slot</returns>
        private int GetSlotValue(int excWord, int index, int excOffset)
        {
            int value;
            if ((excWord & EXC_DOUBLE_SLOTS) == 0)
            {
                excOffset += SlotOffset(excWord, index);
                value = exceptions[excOffset];
            }
            else
            {
                excOffset += 2 * SlotOffset(excWord, index);
                value = exceptions[excOffset++];
                value = (value << 16) | exceptions[excOffset];
            }
            return value;
        }

        // simple case mappings ------------------------------------------------ ***

        public int ToLower(int c)
        {
            int props = trie.Get(c);
            if (!PropsHasException(props))
            {
                if (GetTypeFromProps(props) >= UPPER)
                {
                    c += GetDelta(props);
                }
            }
            else
            {
                int excOffset = GetExceptionsOffset(props);
                int excWord = exceptions[excOffset++];
                if (HasSlot(excWord, EXC_LOWER))
                {
                    c = GetSlotValue(excWord, EXC_LOWER, excOffset);
                }
            }
            return c;
        }

        public int ToUpper(int c)
        {
            int props = trie.Get(c);
            if (!PropsHasException(props))
            {
                if (GetTypeFromProps(props) == LOWER)
                {
                    c += GetDelta(props);
                }
            }
            else
            {
                int excOffset = GetExceptionsOffset(props);
                int excWord = exceptions[excOffset++];
                if (HasSlot(excWord, EXC_UPPER))
                {
                    c = GetSlotValue(excWord, EXC_UPPER, excOffset);
                }
            }
            return c;
        }

        public int ToTitle(int c)
        {
            int props = trie.Get(c);
            if (!PropsHasException(props))
            {
                if (GetTypeFromProps(props) == LOWER)
                {
                    c += GetDelta(props);
                }
            }
            else
            {
                int excOffset = GetExceptionsOffset(props);
                int excWord = exceptions[excOffset++];
                int index;
                if (HasSlot(excWord, EXC_TITLE))
                {
                    index = EXC_TITLE;
                }
                else if (HasSlot(excWord, EXC_UPPER))
                {
                    index = EXC_UPPER;
                }
                else
                {
                    return c;
                }
                c = GetSlotValue(excWord, index, excOffset);
            }
            return c;
        }

        /// <summary>
        /// Adds all simple case mappings and the full case folding for <paramref name="c"/> to sa,
        /// and also adds special case closure mappings.
        /// </summary>
        /// <remarks>
        /// <paramref name="c"/> itself is not added.
        /// For example, the mappings
        /// <list type="bullet">
        ///     <item><description>for s include long s</description></item>
        ///     <item><description>for sharp s include ss</description></item>
        ///     <item><description>for k include the Kelvin sign</description></item>
        /// </list>
        /// </remarks>
        public void AddCaseClosure(int c, UnicodeSet set)
        {
            /*
             * Hardcode the case closure of i and its relatives and ignore the
             * data file data for these characters.
             * The Turkic dotless i and dotted I with their case mapping conditions
             * and case folding option make the related characters behave specially.
             * This code matches their closure behavior to their case folding behavior.
             */

            switch (c)
            {
                case 0x49:
                    /* regular i and I are in one equivalence class */
                    set.Add(0x69);
                    return;
                case 0x69:
                    set.Add(0x49);
                    return;
                case 0x130:
                    /* dotted I is in a class with <0069 0307> (for canonical equivalence with <0049 0307>) */
                    set.Add(iDot);
                    return;
                case 0x131:
                    /* dotless i is in a class by itself */
                    return;
                default:
                    /* otherwise use the data file data */
                    break;
            }

            int props = trie.Get(c);
            if (!PropsHasException(props))
            {
                if (GetTypeFromProps(props) != NONE)
                {
                    /* add the one simple case mapping, no matter what type it is */
                    int delta = GetDelta(props);
                    if (delta != 0)
                    {
                        set.Add(c + delta);
                    }
                }
            }
            else
            {
                /*
                 * c has exceptions, so there may be multiple simple and/or
                 * full case mappings. Add them all.
                 */
                int excOffset0, excOffset = GetExceptionsOffset(props);
                int closureOffset;
                int excWord = exceptions[excOffset++];
                int index, closureLength, fullLength, length;

                excOffset0 = excOffset;

                /* add all simple case mappings */
                for (index = EXC_LOWER; index <= EXC_TITLE; ++index)
                {
                    if (HasSlot(excWord, index))
                    {
                        excOffset = excOffset0;
                        c = GetSlotValue(excWord, index, excOffset);
                        set.Add(c);
                    }
                }

                /* get the closure string pointer & length */
                if (HasSlot(excWord, EXC_CLOSURE))
                {
                    excOffset = excOffset0;
                    long value = GetSlotValueAndOffset(excWord, EXC_CLOSURE, excOffset);
                    closureLength = (int)value & CLOSURE_MAX_LENGTH; /* higher bits are reserved */
                    closureOffset = (int)(value >> 32) + 1; /* behind this slot, unless there are full case mappings */
                }
                else
                {
                    closureLength = 0;
                    closureOffset = 0;
                }

                /* add the full case folding */
                if (HasSlot(excWord, EXC_FULL_MAPPINGS))
                {
                    excOffset = excOffset0;
                    long value = GetSlotValueAndOffset(excWord, EXC_FULL_MAPPINGS, excOffset);
                    fullLength = (int)value;

                    /* start of full case mapping strings */
                    excOffset = (int)(value >> 32) + 1;

                    fullLength &= 0xffff; /* bits 16 and higher are reserved */

                    /* skip the lowercase result string */
                    excOffset += fullLength & FULL_LOWER;
                    fullLength >>= 4;

                    /* add the full case folding string */
                    length = fullLength & 0xf;
                    if (length != 0)
                    {
                        set.Add(exceptions.Substring(excOffset, length)); // ICU4N: excOffset + length - excOffset == length
                        excOffset += length;
                    }

                    /* skip the uppercase and titlecase strings */
                    fullLength >>= 4;
                    excOffset += fullLength & 0xf;
                    fullLength >>= 4;
                    excOffset += fullLength;

                    closureOffset = excOffset; /* behind full case mappings */
                }

                /* add each code point in the closure string */
                int limit = closureOffset + closureLength;
                for (index = closureOffset; index < limit; index += UTF16.GetCharCount(c))
                {
                    c = exceptions.CodePointAt(index);
                    set.Add(c);
                }
            }
        }

        /// <summary>
        /// compare s, which has a length, with t=unfold[unfoldOffset..], which has a maximum length or is NUL-terminated
        /// must be s.Length>0 and max>0 and s.Length&lt;=max
        /// </summary>
        private int StrCmpMax(string s, int unfoldOffset, int max)
        {
            int i1, length, c1, c2;

            length = s.Length;
            max -= length; /* we require length<=max, so no need to decrement max in the loop */
            i1 = 0;
            do
            {
                c1 = s[i1++];
                c2 = unfold[unfoldOffset++];
                if (c2 == 0)
                {
                    return 1; /* reached the end of t but not of s */
                }
                c1 -= c2;
                if (c1 != 0)
                {
                    return c1; /* return difference result */
                }
            } while (--length > 0);
            /* ends with length==0 */

            if (max == 0 || unfold[unfoldOffset] == 0)
            {
                return 0; /* equal to length of both strings */
            }
            else
            {
                return -max; /* return lengh difference */
            }
        }

        /// <summary>
        /// Maps the string to single code points and adds the associated case closure
        /// mappings.
        /// </summary>
        /// <remarks>
        /// The string is mapped to code points if it is their full case folding string.
        /// In other words, this performs a reverse full case folding and then
        /// adds the case closure items of the resulting code points.
        /// If the string is found and its closure applied, then
        /// the string itself is added as well as part of its code points' closure.
        /// </remarks>
        /// <returns>true if the string was found.</returns>
        public bool AddStringCaseClosure(string s, UnicodeSet set)
        {
            int i, length, start, limit, result, unfoldOffset, unfoldRows, unfoldRowWidth, unfoldStringWidth;

            if (unfold == null || s == null)
            {
                return false; /* no reverse case folding data, or no string */
            }
            length = s.Length;
            if (length <= 1)
            {
                /* the string is too short to find any match */
                /*
                 * more precise would be:
                 * if(!u_strHasMoreChar32Than(s, length, 1))
                 * but this does not make much practical difference because
                 * a single supplementary code point would just not be found
                 */
                return false;
            }

            unfoldRows = unfold[UNFOLD_ROWS];
            unfoldRowWidth = unfold[UNFOLD_ROW_WIDTH];
            unfoldStringWidth = unfold[UNFOLD_STRING_WIDTH];
            //unfoldCPWidth=unfoldRowWidth-unfoldStringWidth;

            if (length > unfoldStringWidth)
            {
                /* the string is too long to find any match */
                return false;
            }

            /* do a binary search for the string */
            start = 0;
            limit = unfoldRows;
            while (start < limit)
            {
                i = (start + limit) / 2;
                unfoldOffset = ((i + 1) * unfoldRowWidth); // +1 to skip the header values above
                result = StrCmpMax(s, unfoldOffset, unfoldStringWidth);

                if (result == 0)
                {
                    /* found the string: add each code point, and its case closure */
                    int c;

                    for (i = unfoldStringWidth; i < unfoldRowWidth && unfold[unfoldOffset + i] != 0; i += UTF16.GetCharCount(c))
                    {
                        c = UTF16.CharAt(unfold, unfoldOffset, unfold.Length, i);
                        set.Add(c);
                        AddCaseClosure(c, set);
                    }
                    return true;
                }
                else if (result < 0)
                {
                    limit = i;
                }
                else /* result>0 */
                {
                    start = i + 1;
                }
            }

            return false; /* string not found */
        }

        /// <returns><see cref="NONE"/>, <see cref="LOWER"/>, <see cref="UPPER"/>, <see cref="TITLE"/></returns>
        public int GetType(int c)
        {
            return GetTypeFromProps(trie.Get(c));
        }

        /// <summary>
        /// Like <see cref="GetType(int)"/>, but also sets <see cref="IGNORABLE"/> if <paramref name="c"/> is case-ignorable.
        /// </summary>
        public int GetTypeOrIgnorable(int c)
        {
            return GetTypeAndIgnorableFromProps(trie.Get(c));
        }

        /// <returns><see cref="NO_DOT"/>, <see cref="SOFT_DOTTED"/>, <see cref="ABOVE"/>, <see cref="OTHER_ACCENT"/>.</returns>
        public int GetDotType(int c)
        {
            int props = trie.Get(c);
            if (!PropsHasException(props))
            {
                return props & DOT_MASK;
            }
            else
            {
                return (exceptions[GetExceptionsOffset(props)] >> EXC_DOT_SHIFT) & DOT_MASK;
            }
        }

        public bool IsSoftDotted(int c)
        {
            return GetDotType(c) == SOFT_DOTTED;
        }

        public bool IsCaseSensitive(int c)
        {
            return (trie.Get(c) & SENSITIVE) != 0;
        }

        // string casing ------------------------------------------------------- ***

        /*
         * These internal functions form the core of string case mappings.
         * They map single code points to result code points or strings and take
         * all necessary conditions (context, locale ID, options) into account.
         *
         * They do not iterate over the source or write to the destination
         * so that the same functions are useful for non-standard string storage,
         * such as in a Replaceable (for Transliterator) or UTF-8/32 strings etc.
         * For the same reason, the "surrounding text" context is passed in as a
         * ContextIterator which does not make any assumptions about
         * the underlying storage.
         *
         * This section contains helper functions that check for conditions
         * in the input text surrounding the current code point
         * according to SpecialCasing.txt.
         *
         * Each helper function gets the index
         * - after the current code point if it looks at following text
         * - before the current code point if it looks at preceding text
         *
         * Unicode 3.2 UAX 21 "Case Mappings" defines the conditions as follows:
         *
         * Final_Sigma
         *   C is preceded by a sequence consisting of
         *     a cased letter and a case-ignorable sequence,
         *   and C is not followed by a sequence consisting of
         *     an ignorable sequence and then a cased letter.
         *
         * More_Above
         *   C is followed by one or more characters of combining class 230 (ABOVE)
         *   in the combining character sequence.
         *
         * After_Soft_Dotted
         *   The last preceding character with combining class of zero before C
         *   was Soft_Dotted,
         *   and there is no intervening combining character class 230 (ABOVE).
         *
         * Before_Dot
         *   C is followed by combining dot above (U+0307).
         *   Any sequence of characters with a combining class that is neither 0 nor 230
         *   may intervene between the current character and the combining dot above.
         *
         * The erratum from 2002-10-31 adds the condition
         *
         * After_I
         *   The last preceding base character was an uppercase I, and there is no
         *   intervening combining character class 230 (ABOVE).
         *
         *   (See Jitterbug 2344 and the comments on After_I below.)
         *
         * Helper definitions in Unicode 3.2 UAX 21:
         *
         * D1. A character C is defined to be cased
         *     if it meets any of the following criteria:
         *
         *   - The general category of C is Titlecase Letter (Lt)
         *   - In [CoreProps], C has one of the properties Uppercase, or Lowercase
         *   - Given D = NFD(C), then it is not the case that:
         *     D = UCD_lower(D) = UCD_upper(D) = UCD_title(D)
         *     (This third criterium does not add any characters to the list
         *      for Unicode 3.2. Ignored.)
         *
         * D2. A character C is defined to be case-ignorable
         *     if it meets either of the following criteria:
         *
         *   - The general category of C is
         *     Nonspacing Mark (Mn), or Enclosing Mark (Me), or Format Control (Cf), or
         *     Letter Modifier (Lm), or Symbol Modifier (Sk)
         *   - C is one of the following characters
         *     U+0027 APOSTROPHE
         *     U+00AD SOFT HYPHEN (SHY)
         *     U+2019 RIGHT SINGLE QUOTATION MARK
         *            (the preferred character for apostrophe)
         *
         * D3. A case-ignorable sequence is a sequence of
         *     zero or more case-ignorable characters.
         */

        /// <summary>
        /// Iterator for string case mappings, which need to look at the
        /// context (surrounding text) of a given character for conditional mappings.
        /// </summary>
        /// <remarks>
        /// The iterator only needs to go backward or forward away from the
        /// character in question. It does not use any indexes on this interface.
        /// It does not support random access or an arbitrary change of
        /// iteration direction.
        /// <para/>
        /// The code point being case-mapped itself is never returned by
        /// this iterator.
        /// </remarks>
        public interface IContextIterator // ICU4N TODO: API De-nest this interface
        {
            /// <summary>
            /// Reset the iterator for forward or backward iteration.
            /// </summary>
            /// <param name="dir">
            /// >0: Begin iterating forward from the first code point
            /// after the one that is being case-mapped.
            /// &lt;0: Begin iterating backward from the first code point
            /// before the one that is being case-mapped.
            /// </param>
            void Reset(int dir);

            /// <summary>
            /// Iterate and return the next code point, moving in the direction
            /// determined by the <see cref="Reset(int)"/> call.
            /// </summary>
            /// <returns>Next code point, or &lt;0 when the iteration is done.</returns>
            int Next();
        }

        /// <summary>
        /// For string case mappings, a single character (a code point) is mapped
        /// either to itself (in which case in-place mapping functions do nothing),
        /// or to another single code point, or to a string.
        /// Aside from the string contents, these are indicated with a single int
        /// value as follows:
        /// <list type="table">
        ///     <item><term>Mapping to self</term><description>Negative values (~self instead of -self to support U+0000)</description></item>
        ///     <item><term>Mapping to another code point</term><description>Positive values ><see cref="MAX_STRING_LENGTH"/></description></item>
        ///     <item><term>Mapping to a string</term><description>
        ///         The string length (0..MAX_STRING_LENGTH) is
        ///         returned. Note that the string result may indeed have zero length.
        ///     </description></item>
        /// </list>
        /// </summary>
        public static readonly int MAX_STRING_LENGTH = 0x1f;

        //private static readonly int LOC_UNKNOWN=0;
        public static readonly int LOC_ROOT = 1;
        private static readonly int LOC_TURKISH = 2;
        private static readonly int LOC_LITHUANIAN = 3;
        internal static readonly int LOC_GREEK = 4;
        public static readonly int LOC_DUTCH = 5;

        public static int GetCaseLocale(CultureInfo locale)
        {
            return GetCaseLocale(locale.TwoLetterISOLanguageName);
        }
        public static int GetCaseLocale(ULocale locale)
        {
            return GetCaseLocale(locale.GetLanguage());
        }
        /// <summary>Accepts both 2- and 3-letter language subtags.</summary>
        private static int GetCaseLocale(string language)
        {
            // Check the subtag length to reduce the number of comparisons
            // for locales without special behavior.
            // Fastpath for English "en" which is often used for default (=root locale) case mappings,
            // and for Chinese "zh": Very common but no special case mapping behavior.
            if (language.Length == 2)
            {
                if (language.Equals("en") || language[0] > 't')
                {
                    return LOC_ROOT;
                }
                else if (language.Equals("tr") || language.Equals("az"))
                {
                    return LOC_TURKISH;
                }
                else if (language.Equals("el"))
                {
                    return LOC_GREEK;
                }
                else if (language.Equals("lt"))
                {
                    return LOC_LITHUANIAN;
                }
                else if (language.Equals("nl"))
                {
                    return LOC_DUTCH;
                }
            }
            else if (language.Length == 3)
            {
                if (language.Equals("tur") || language.Equals("aze"))
                {
                    return LOC_TURKISH;
                }
                else if (language.Equals("ell"))
                {
                    return LOC_GREEK;
                }
                else if (language.Equals("lit"))
                {
                    return LOC_LITHUANIAN;
                }
                else if (language.Equals("nld"))
                {
                    return LOC_DUTCH;
                }
            }
            return LOC_ROOT;
        }

        /// <summary>Is followed by {case-ignorable}* cased  ? (dir determines looking forward/backward)</summary>
        private bool IsFollowedByCasedLetter(IContextIterator iter, int dir)
        {
            int c;

            if (iter == null)
            {
                return false;
            }

            for (iter.Reset(dir); (c = iter.Next()) >= 0;)
            {
                int type = GetTypeOrIgnorable(c);
                if ((type & 4) != 0)
                {
                    /* case-ignorable, continue with the loop */
                }
                else if (type != NONE)
                {
                    return true; /* followed by cased letter */
                }
                else
                {
                    return false; /* uncased and not case-ignorable */
                }
            }

            return false; /* not followed by cased letter */
        }

        /// <summary>Is preceded by Soft_Dotted character with no intervening cc=230 ?</summary>
        private bool IsPrecededBySoftDotted(IContextIterator iter)
        {
            int c;
            int dotType;

            if (iter == null)
            {
                return false;
            }

            for (iter.Reset(-1); (c = iter.Next()) >= 0;)
            {
                dotType = GetDotType(c);
                if (dotType == SOFT_DOTTED)
                {
                    return true; /* preceded by TYPE_i */
                }
                else if (dotType != OTHER_ACCENT)
                {
                    return false; /* preceded by different base character (not TYPE_i), or intervening cc==230 */
                }
            }

            return false; /* not preceded by TYPE_i */
        }

        /*
         * See Jitterbug 2344:
         * The condition After_I for Turkic-lowercasing of U+0307 combining dot above
         * is checked in ICU 2.0, 2.1, 2.6 but was not in 2.2 & 2.4 because
         * we made those releases compatible with Unicode 3.2 which had not fixed
         * a related bug in SpecialCasing.txt.
         *
         * From the Jitterbug 2344 text:
         * ... this bug is listed as a Unicode erratum
         * from 2002-10-31 at http://www.unicode.org/uni2errata/UnicodeErrata.html
         * <quote>
         * There are two errors in SpecialCasing.txt.
         * 1. Missing semicolons on two lines. ... [irrelevant for ICU]
         * 2. An incorrect context definition. Correct as follows:
         * < 0307; ; 0307; 0307; tr After_Soft_Dotted; # COMBINING DOT ABOVE
         * < 0307; ; 0307; 0307; az After_Soft_Dotted; # COMBINING DOT ABOVE
         * ---
         * > 0307; ; 0307; 0307; tr After_I; # COMBINING DOT ABOVE
         * > 0307; ; 0307; 0307; az After_I; # COMBINING DOT ABOVE
         * where the context After_I is defined as:
         * The last preceding base character was an uppercase I, and there is no
         * intervening combining character class 230 (ABOVE).
         * </quote>
         *
         * Note that SpecialCasing.txt even in Unicode 3.2 described the condition as:
         *
         * # When lowercasing, remove dot_above in the sequence I + dot_above, which will turn into i.
         * # This matches the behavior of the canonically equivalent I-dot_above
         *
         * See also the description in this place in older versions of uchar.c (revision 1.100).
         *
         * Markus W. Scherer 2003-feb-15
         */

        /// <summary>Is preceded by base character 'I' with no intervening cc=230 ?</summary>
        private bool IsPrecededBy_I(IContextIterator iter)
        {
            int c;
            int dotType;

            if (iter == null)
            {
                return false;
            }

            for (iter.Reset(-1); (c = iter.Next()) >= 0;)
            {
                if (c == 0x49)
                {
                    return true; /* preceded by I */
                }
                dotType = GetDotType(c);
                if (dotType != OTHER_ACCENT)
                {
                    return false; /* preceded by different base character (not I), or intervening cc==230 */
                }
            }

            return false; /* not preceded by I */
        }

        /// <summary>Is followed by one or more cc==230 ?</summary>
        private bool IsFollowedByMoreAbove(IContextIterator iter)
        {
            int c;
            int dotType;

            if (iter == null)
            {
                return false;
            }

            for (iter.Reset(1); (c = iter.Next()) >= 0;)
            {
                dotType = GetDotType(c);
                if (dotType == ABOVE)
                {
                    return true; /* at least one cc==230 following */
                }
                else if (dotType != OTHER_ACCENT)
                {
                    return false; /* next base character, no more cc==230 following */
                }
            }

            return false; /* no more cc==230 following */
        }

        /// <summary>Is followed by a dot above (without cc==230 in between) ?</summary>
        private bool IsFollowedByDotAbove(IContextIterator iter)
        {
            int c;
            int dotType;

            if (iter == null)
            {
                return false;
            }

            for (iter.Reset(1); (c = iter.Next()) >= 0;)
            {
                if (c == 0x307)
                {
                    return true;
                }
                dotType = GetDotType(c);
                if (dotType != OTHER_ACCENT)
                {
                    return false; /* next base character or cc==230 in between */
                }
            }

            return false; /* no dot above following */
        }

        private static readonly string
                iDot = "i\u0307",
                jDot = "j\u0307",
                iOgonekDot = "\u012f\u0307",
                iDotGrave = "i\u0307\u0300",
                iDotAcute = "i\u0307\u0301",
                iDotTilde = "i\u0307\u0303";

        // ICU4N specific - ToFullLower(int c, IContextIterator iter, IAppendable output, int caseLocale) moved to UCasePropsExtension.tt

        // ICU4N specific - ToUpperOrTitle(int c, IContextIterator iter,
        //    IAppendable output,
        //    int loc,
        //    bool upperNotTitle) moved to UCasePropsExtension.tt

        // ICU4N specific - ToFullUpper(int c, IContextIterator iter,
        //    IAppendable output,
        //    int caseLocale) moved to UCasePropsExtension.tt

        // ICU4N specific - ToFullTitle(int c, IContextIterator iter,
        //    IAppendable output,
        //    int caseLocale) moved to UCasePropsExtension.tt




        /* case folding ------------------------------------------------------------- */

        /*
         * Case folding is similar to lowercasing.
         * The result may be a simple mapping, i.e., a single code point, or
         * a full mapping, i.e., a string.
         * If the case folding for a code point is the same as its simple (1:1) lowercase mapping,
         * then only the lowercase mapping is stored.
         *
         * Some special cases are hardcoded because their conditions cannot be
         * parsed and processed from CaseFolding.txt.
         *
         * Unicode 3.2 CaseFolding.txt specifies for its status field:

        # C: common case folding, common mappings shared by both simple and full mappings.
        # F: full case folding, mappings that cause strings to grow in length. Multiple characters are separated by spaces.
        # S: simple case folding, mappings to single characters where different from F.
        # T: special case for uppercase I and dotted uppercase I
        #    - For non-Turkic languages, this mapping is normally not used.
        #    - For Turkic languages (tr, az), this mapping can be used instead of the normal mapping for these characters.
        #
        # Usage:
        #  A. To do a simple case folding, use the mappings with status C + S.
        #  B. To do a full case folding, use the mappings with status C + F.
        #
        #    The mappings with status T can be used or omitted depending on the desired case-folding
        #    behavior. (The default option is to exclude them.)

         * Unicode 3.2 has 'T' mappings as follows:

        0049; T; 0131; # LATIN CAPITAL LETTER I
        0130; T; 0069; # LATIN CAPITAL LETTER I WITH DOT ABOVE

         * while the default mappings for these code points are:

        0049; C; 0069; # LATIN CAPITAL LETTER I
        0130; F; 0069 0307; # LATIN CAPITAL LETTER I WITH DOT ABOVE

         * U+0130 has no simple case folding (simple-case-folds to itself).
         */

        /// <summary>
        /// Bit mask for getting just the options from a string compare options word
        /// that are relevant for case folding (of a single string or code point).
        /// </summary>
        /// <remarks>
        /// Currently only bit 0 for <see cref="UCharacter.FOLD_CASE_EXCLUDE_SPECIAL_I"/>.
        /// It is conceivable that at some point we might use one more bit for using uppercase sharp s.
        /// It is conceivable that at some point we might want the option to use only simple case foldings
        /// when operating on strings.
        /// </remarks>
        /// <internal/>
        private static readonly int FOLD_CASE_OPTIONS_MASK = 7;

        /// <summary>Returns the simple case folding mapping for <paramref name="c"/>.</summary>
        public int Fold(int c, int options)
        {
            int props = trie.Get(c);
            if (!PropsHasException(props))
            {
                if (GetTypeFromProps(props) >= UPPER)
                {
                    c += GetDelta(props);
                }
            }
            else
            {
                int excOffset = GetExceptionsOffset(props);
                int excWord = exceptions[excOffset++];
                int index;
                if ((excWord & EXC_CONDITIONAL_FOLD) != 0)
                {
                    /* special case folding mappings, hardcoded */
                    if ((options & FOLD_CASE_OPTIONS_MASK) == UCharacter.FOLD_CASE_DEFAULT)
                    {
                        /* default mappings */
                        if (c == 0x49)
                        {
                            /* 0049; C; 0069; # LATIN CAPITAL LETTER I */
                            return 0x69;
                        }
                        else if (c == 0x130)
                        {
                            /* no simple case folding for U+0130 */
                            return c;
                        }
                    }
                    else
                    {
                        /* Turkic mappings */
                        if (c == 0x49)
                        {
                            /* 0049; T; 0131; # LATIN CAPITAL LETTER I */
                            return 0x131;
                        }
                        else if (c == 0x130)
                        {
                            /* 0130; T; 0069; # LATIN CAPITAL LETTER I WITH DOT ABOVE */
                            return 0x69;
                        }
                    }
                }
                if (HasSlot(excWord, EXC_FOLD))
                {
                    index = EXC_FOLD;
                }
                else if (HasSlot(excWord, EXC_LOWER))
                {
                    index = EXC_LOWER;
                }
                else
                {
                    return c;
                }
                c = GetSlotValue(excWord, index, excOffset);
            }
            return c;
        }

        // ICU4N specific - ToFullFolding(int c, IAppendable output, int options) moved to UCasePropsExtension.tt



        /* case mapping properties API ---------------------------------------------- */

        
        private static StringBuilder dummyStringBuilder = new StringBuilder();

        /// <summary>
        /// We need a <see cref="StringBuilder"/> for multi-code point output from the
        /// full case mapping functions. However, we do not actually use that output,
        /// we just check whether the input character was mapped to anything else.
        /// We use a shared <see cref="StringBuilder"/> to avoid allocating a new one in each call.
        /// We remove its contents each time so that it does not grow large over time.
        /// </summary>
        public static StringBuilder DummyStringBuilder { get { return dummyStringBuilder; } }

        public bool HasBinaryProperty(int c, int which)
        {
            switch (which)
            {
                case (int)UProperty.Lowercase:
                    return LOWER == GetType(c);
                case (int)UProperty.Uppercase:
                    return UPPER == GetType(c);
                case (int)UProperty.Soft_Dotted:
                    return IsSoftDotted(c);
                case (int)UProperty.Case_Sensitive:
                    return IsCaseSensitive(c);
                case (int)UProperty.Cased:
                    return NONE != GetType(c);
                case (int)UProperty.Case_Ignorable:
                    return (GetTypeOrIgnorable(c) >> 2) != 0;
                /*
                 * Note: The following Changes_When_Xyz are defined as testing whether
                 * the NFD form of the input changes when Xyz-case-mapped.
                 * However, this simpler implementation of these properties,
                 * ignoring NFD, passes the tests.
                 * The implementation needs to be changed if the tests start failing.
                 * When that happens, optimizations should be used to work with the
                 * per-single-code point ucase_toFullXyz() functions unless
                 * the NFD form has more than one code point,
                 * and the property starts set needs to be the union of the
                 * start sets for normalization and case mappings.
                 */
                case (int)UProperty.Changes_When_Lowercased:
                    dummyStringBuilder.Length = 0;
                    return ToFullLower(c, null, dummyStringBuilder, LOC_ROOT) >= 0;
                case (int)UProperty.Changes_When_Uppercased:
                    dummyStringBuilder.Length = 0;
                    return ToFullUpper(c, null, dummyStringBuilder, LOC_ROOT) >= 0;
                case (int)UProperty.Changes_When_Titlecased:
                    dummyStringBuilder.Length = 0;
                    return ToFullTitle(c, null, dummyStringBuilder, LOC_ROOT) >= 0;
                /* case UProperty.CHANGES_WHEN_CASEFOLDED: -- in UCharacterProperty.java */
                case (int)UProperty.Changes_When_Casemapped:
                    dummyStringBuilder.Length = 0;
                    return
                ToFullLower(c, null, dummyStringBuilder, LOC_ROOT) >= 0 ||
                ToFullUpper(c, null, dummyStringBuilder, LOC_ROOT) >= 0 ||
                ToFullTitle(c, null, dummyStringBuilder, LOC_ROOT) >= 0;
                default:
                    return false;
            }
        }

        // data members -------------------------------------------------------- ***
        private int[] indexes;
        private String exceptions;
        private char[] unfold;

        private Trie2_16 trie;

        // data format constants ----------------------------------------------- ***
        private static readonly String DATA_NAME = "ucase";
        private static readonly String DATA_TYPE = "icu";
        private static readonly String DATA_FILE_NAME = DATA_NAME + "." + DATA_TYPE;

        /// <summary>format "cAsE"</summary>
        private static readonly int FMT = 0x63415345;

        /* indexes into indexes[] */
        //private static readonly int IX_INDEX_TOP=0;
        //private static readonly int IX_LENGTH=1;
        private static readonly int IX_TRIE_SIZE = 2;
        private static readonly int IX_EXC_LENGTH = 3;
        private static readonly int IX_UNFOLD_LENGTH = 4;

        //private static readonly int IX_MAX_FULL_LENGTH=15;
        private static readonly int IX_TOP = 16;

        // definitions for 16-bit case properties word ------------------------- ***

        /* 2-bit constants for types of cased characters */
        public static readonly int TYPE_MASK = 3;
        public static readonly int NONE = 0; // ICU4N TODO: Make into [Flags] enum ?
        public static readonly int LOWER = 1;
        public static readonly int UPPER = 2;
        public static readonly int TITLE = 3;

        /// <returns><see cref="NONE"/>, <see cref="LOWER"/>, <see cref="UPPER"/>, <see cref="TITLE"/></returns>
        private static int GetTypeFromProps(int props)
        {
            return props & TYPE_MASK;
        }

        /// <summary>
        /// Like <see cref="GetTypeFromProps(int)"/>, but also sets <see cref="IGNORABLE"/> if <paramref name="props"/> indicate case-ignorable.
        /// </summary>
        private static int GetTypeAndIgnorableFromProps(int props)
        {
            return props & 7;
        }

        internal static readonly int IGNORABLE = 4;
        private static readonly int SENSITIVE = 8;
        private static readonly int EXCEPTION = 0x10;

        // ICU4N TODO: API - make into [Flags] enum ?
        private static readonly int DOT_MASK = 0x60;
        //private static readonly int NO_DOT=        0;      /* normal characters with cc=0 */
        private static readonly int SOFT_DOTTED = 0x20;   /* soft-dotted characters with cc=0 */
        private static readonly int ABOVE = 0x40;   /* "above" accents with cc=230 */
        private static readonly int OTHER_ACCENT = 0x60;   /* other accent character (0<cc!=230) */

        /* no exception: bits 15..7 are a 9-bit signed case mapping delta */
        private static readonly int DELTA_SHIFT = 7;
        //private static readonly int DELTA_MASK=    0xff80;
        //private static readonly int MAX_DELTA=     0xff;
        //private static readonly int MIN_DELTA=     (-MAX_DELTA-1);

        private static int GetDelta(int props)
        {
            return (short)props >> DELTA_SHIFT;
        }

        /// <summary>exception: bits 15..5 are an unsigned 11-bit index into the exceptions array</summary>
        private static readonly int EXC_SHIFT = 5;
        //private static readonly int EXC_MASK=      0xffe0;
        //private static readonly int MAX_EXCEPTIONS=((EXC_MASK>>EXC_SHIFT)+1);

        /* definitions for 16-bit main exceptions word ------------------------------ */

        /// <summary>first 8 bits indicate values in optional slots</summary>
        private static readonly int EXC_LOWER = 0;
        private static readonly int EXC_FOLD = 1;
        private static readonly int EXC_UPPER = 2;
        private static readonly int EXC_TITLE = 3;
        //private static readonly int EXC_4=4;           /* reserved */
        //private static readonly int EXC_5=5;           /* reserved */
        private static readonly int EXC_CLOSURE = 6;
        private static readonly int EXC_FULL_MAPPINGS = 7;
        //private static readonly int EXC_ALL_SLOTS=8;   /* one past the last slot */

        /// <summary>each slot is 2 uint16_t instead of 1</summary>
        private static readonly int EXC_DOUBLE_SLOTS = 0x100;

        /* reserved: exception bits 11..9 */

        /// <summary>EXC_DOT_MASK=<see cref="DOT_MASK"/>&lt;&lt;<see cref="EXC_DOT_SHIFT"/></summary>
        private static readonly int EXC_DOT_SHIFT = 7;

        /* normally stored in the main word, but pushed out for larger exception indexes */
        //private static readonly int EXC_DOT_MASK=              0x3000;
        //private static readonly int EXC_NO_DOT=                0;
        //private static readonly int EXC_SOFT_DOTTED=           0x1000;
        //private static readonly int EXC_ABOVE=                 0x2000; /* "above" accents with cc=230 */
        //private static readonly int EXC_OTHER_ACCENT=          0x3000; /* other character (0<cc!=230) */

        /* complex/conditional mappings */
        private static readonly int EXC_CONDITIONAL_SPECIAL = 0x4000;
        private static readonly int EXC_CONDITIONAL_FOLD = 0x8000;

        /* definitions for lengths word for full case mappings */
        private static readonly int FULL_LOWER = 0xf;
        //private static readonly int FULL_FOLDING=  0xf0;
        //private static readonly int FULL_UPPER=    0xf00;
        //private static readonly int FULL_TITLE=    0xf000;

        /* maximum lengths */
        //private static readonly int FULL_MAPPINGS_MAX_LENGTH=4*0xf;
        private static readonly int CLOSURE_MAX_LENGTH = 0xf;

        /* constants for reverse case folding ("unfold") data */
        private static readonly int UNFOLD_ROWS = 0;
        private static readonly int UNFOLD_ROW_WIDTH = 1;
        private static readonly int UNFOLD_STRING_WIDTH = 2;

        /// <summary>
        /// Public singleton instance.
        /// </summary>
        public static readonly UCaseProps INSTANCE;

        // This static initializer block must be placed after
        // other static member initialization
        static UCaseProps()
        {
            try
            {
                INSTANCE = new UCaseProps();
            }
            catch (IOException e)
            {
                throw new ICUUncheckedIOException(e);
            }
        }
    }
}
