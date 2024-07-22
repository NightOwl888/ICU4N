using ICU4N.Globalization;
using ICU4N.Impl.Locale;
using ICU4N.Support.Text;
using ICU4N.Text;
using ICU4N.Util;
using J2N;
using J2N.IO;
using J2N.Text;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
#nullable enable

namespace ICU4N.Impl
{
    // ICU4N specific - Instead of using ContextIterator from ICU4J, ported the
    // UCaseContextIterator directly from ICU4C. Although not as user-friendly,
    // this is more flexible because it doesn't limit us to using the heap to
    // allocate strings. This means we can support ReadOnlySpan<char> as well
    // as classes and structs on the the heap.
    /// <summary>
    /// Iterator function for string case mappings, which need to look at the
    /// context (surrounding text) of a given character for conditional mappings.
    /// <para/>
    /// The iterator only needs to go backward or forward away from the
    /// character in question. It does not use any indexes on this interface
    /// It does not support random access or an arbitrary change of
    /// iteration direction.
    /// <para/>
    /// The code point being case-mapped itself is never returned by
    /// this iterator.
    /// </summary>
    /// <param name="context">A pointer to the iterator's working data.
    /// This may be a ref struct, struct, or a class.</param>
    /// <param name="direction">If &lt;0 then start iterating backward from the character;
    ///                         if &gt;0 then start iterating forward from the character;
    ///                         if 0 then continue iterating in the current direction.
    ///            </param>
    /// <returns>Next code point, or &lt;0 when the iteration is done.</returns>
    // ICU4N: Ported from ucase.h instead of using an interface, which would bind us to the heap.
    [CLSCompliant(false)]
    public delegate int UCaseContextIterator(IntPtr context, sbyte direction); // ICU4N: In C++ context was a void*, but in C# we use IntPtr to allow for managed types to be used.

    /// <summary>
    /// Sample struct which may be used by some implementations of
    /// <see cref="UCaseContextIterator"/>.
    /// </summary>
    public ref struct UCaseContext
    {
        [CLSCompliant(false)]
        public IntPtr p; // ICU4N: void* in C++. In C# we use IntPtr to allow managed types, since this may be an IReplaceable instance.
        public int start, index, limit;
        public int cpStart, cpLimit;
        [CLSCompliant(false)]
        public sbyte dir;
        public bool b1, b2, b3;
    }

    /// <internal/>
    // ICU4N: In C++ context was a void*, but in C# we use IntPtr to allow for managed types to be used.
    internal delegate int UCaseMapFull(int c, UCaseContextIterator? iter, IntPtr context, ref ValueStringBuilder output, CaseLocale caseLocale);

    /// <summary>
    /// Casing locale types for <see cref="UCaseProperties.GetCaseLocale(string)"/>,
    /// <see cref="UCaseProperties.GetCaseLocale(CultureInfo)"/> and <see cref="UCaseProperties.GetCaseLocale(UCultureInfo)"/>.
    /// </summary>
    public enum CaseLocale
    {
        // Unknown = 0,
        Root = 1,
        Turkish = 2,
        Lithuanian = 3,
        Greek = 4,
        Dutch = 5
    }

    /// <summary>
    /// Case type for non-case-ignorable properties.
    /// </summary>
    /// <seealso cref="UCaseProperties.GetCaseType(int)"/>
    /// <seealso cref="UCaseProperties.IsCaseIgnorable(int, out CaseType)"/>
    public enum CaseType
    {
        None = 0,
        Lower = 1,
        Upper = 2,
        Title = 3,
    }

    /// <summary>
    /// Dot type for case properties.
    /// </summary>
    /// <seealso cref="UCaseProperties.GetDotType(int)"/>
    [SuppressMessage("Microsoft.Design", "CA1027", Justification = "Enum values cannot be combined")]
    public enum DotType
    {
        NoDot = 0,           /* normal characters with cc=0 */
        SoftDotted = 0x20,   /* soft-dotted characters with cc=0 */
        Above = 0x40,        /* "above" accents with cc=230 */
        OtherAccent = 0x60,  /* other accent character (0<cc!=230) */
    }

    /// <summary>
    /// Low-level Unicode character/string case mapping code.
    /// .NET port of ucase.h/.c.
    /// </summary>
    /// <author>Markus W. Scherer</author>
    /// <created>2005jan29</created>
    public sealed partial class UCaseProperties
    {
        private const int CharStackBufferSize = 32;

        // constructors etc. --------------------------------------------------- ***

        // port of ucase_openProps()
        private UCaseProperties()
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
            int trieLength = trie.SerializedLength;
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
            if (set is null)
                throw new ArgumentNullException(nameof(set)); // ICU4N: Added guard clause

            /* add the start code point of each same-value range of the trie */
            using (var trieIterator = trie.GetEnumerator())
            {
                Trie2Range range;
                while (trieIterator.MoveNext() && !(range = trieIterator.Current).IsLeadSurrogate)
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
                if (GetCaseTypeFromProps(props) >= CaseType.Upper)
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
                if (GetCaseTypeFromProps(props) == CaseType.Lower)
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
                if (GetCaseTypeFromProps(props) == CaseType.Lower)
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
            if (set is null)
                throw new ArgumentNullException(nameof(set)); // ICU4N: Added guard clause

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
                if (GetCaseTypeFromProps(props) != CaseType.None)
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
            if (set is null)
                throw new ArgumentNullException(nameof(set)); // ICU4N: Added guard clause

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
                        c = UTF16.CharAt(unfold.AsSpan(unfoldOffset), i); // ICU4N: changed to ReadOnlySpan<char> overload
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

        /// <returns><see cref="CaseType.None"/>, <see cref="CaseType.Lower"/>, <see cref="CaseType.Upper"/>, <see cref="CaseType.Title"/></returns>
        public CaseType GetCaseType(int c)
        {
            return GetCaseTypeFromProps(trie.Get(c));
        }

        /// <summary>
        /// Like <see cref="GetCaseType(int)"/>, but returns <c>true</c> if <paramref name="c"/> is case-ignorable
        /// and returns <paramref name="type"/> as an out parameter.
        /// </summary>
        // ICU4N specific - rather than returning 2 bits in an int in GetTypeOrIgnorable, 
        // we return ignorable as a bool and type as an out parameter, which makes usage simpler for end users.
        // For performance reasons, it still works best to get these 2 values in one go rather than making 2
        // separate functions.
        public bool IsCaseIgnorable(int c, out CaseType type)
        {
            return IsCaseIgnorableFromProps(trie.Get(c), out type);
        }

        /// <returns><see cref="DotType.NoDot"/>, <see cref="DotType.SoftDotted"/>, <see cref="DotType.Above"/>, <see cref="DotType.OtherAccent"/>.</returns>
        public DotType GetDotType(int c)
        {
            int props = trie.Get(c);
            if (!PropsHasException(props))
            {
                return (DotType)(props & DOT_MASK);
            }
            else
            {
                return (DotType)((exceptions[GetExceptionsOffset(props)] >> EXC_DOT_SHIFT) & DOT_MASK);
            }
        }

        public bool IsSoftDotted(int c)
        {
            return GetDotType(c) == DotType.SoftDotted;
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
        /// For string case mappings, a single character (a code point) is mapped
        /// either to itself (in which case in-place mapping functions do nothing),
        /// or to another single code point, or to a string.
        /// Aside from the string contents, these are indicated with a single int
        /// value as follows:
        /// <list type="table">
        ///     <item><term>Mapping to self</term><description>Negative values (~self instead of -self to support U+0000)</description></item>
        ///     <item><term>Mapping to another code point</term><description>Positive values ><see cref="MaxStringLength"/></description></item>
        ///     <item><term>Mapping to a string</term><description>
        ///         The string length (0..<see cref="MaxStringLength"/>) is
        ///         returned. Note that the string result may indeed have zero length.
        ///     </description></item>
        /// </list>
        /// </summary>
        public const int MaxStringLength = 0x1f;

        // ICU4N specific - moved LOC_ constants to CaseLocale enum and de-nested

        public static CaseLocale GetCaseLocale(CultureInfo locale)
        {
            if (locale is null)
                throw new ArgumentNullException(nameof(locale)); // ICU4N: Added guard clause

            return GetCaseLocale(locale.TwoLetterISOLanguageName);
        }
        public static CaseLocale GetCaseLocale(UCultureInfo locale)
        {
            if (locale is null)
                throw new ArgumentNullException(nameof(locale)); // ICU4N: Added guard clause

            return GetCaseLocale(locale.Language);
        }
        /// <summary>Accepts both 2- and 3-letter language subtags.</summary>
        internal static CaseLocale GetCaseLocale(string language) // ICU4N: Made internal so UCultureData can access and store CaseLocale.
        {
            // Check the subtag length to reduce the number of comparisons
            // for locales without special behavior.
            // Fastpath for English "en" which is often used for default (=root locale) case mappings,
            // and for Chinese "zh": Very common but no special case mapping behavior.
            if (language.Length == 2)
            {
                if (language.Equals("en") || language[0] > 't')
                {
                    return CaseLocale.Root;
                }
                else if (language.Equals("tr") || language.Equals("az"))
                {
                    return CaseLocale.Turkish;
                }
                else if (language.Equals("el"))
                {
                    return CaseLocale.Greek;
                }
                else if (language.Equals("lt"))
                {
                    return CaseLocale.Lithuanian;
                }
                else if (language.Equals("nl"))
                {
                    return CaseLocale.Dutch;
                }
            }
            else if (language.Length == 3)
            {
                if (language.Equals("tur") || language.Equals("aze"))
                {
                    return CaseLocale.Turkish;
                }
                else if (language.Equals("ell"))
                {
                    return CaseLocale.Greek;
                }
                else if (language.Equals("lit"))
                {
                    return CaseLocale.Lithuanian;
                }
                else if (language.Equals("nld"))
                {
                    return CaseLocale.Dutch;
                }
            }
            return CaseLocale.Root;
        }

        /// <summary>Is followed by {case-ignorable}* cased  ? (dir determines looking forward/backward)</summary>
        private bool IsFollowedByCasedLetter(UCaseContextIterator? iter, IntPtr context, sbyte dir)
        {
            int c;

            if (iter == null)
            {
                return false;
            }

            for (/* dir!=0 sets direction */; (c = iter(context, dir)) >= 0; dir = 0)
            {
                // ICU4N: Simplfied version of GetTypeOrIgnorable
                if (IsCaseIgnorable(c, out CaseType type))
                {
                    /* case-ignorable, continue with the loop */
                }
                else if (type != CaseType.None)
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
        private bool IsPrecededBySoftDotted(UCaseContextIterator? iter, IntPtr context)
        {
            int c;
            DotType dotType;
            sbyte dir;

            if (iter == null)
            {
                return false;
            }

            for (dir = -1; (c = iter(context, dir)) >= 0; dir = 0)
            {
                dotType = GetDotType(c);
                if (dotType == DotType.SoftDotted)
                {
                    return true; /* preceded by TYPE_i */
                }
                else if (dotType != DotType.OtherAccent)
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
        private bool IsPrecededBy_I(UCaseContextIterator? iter, IntPtr context)
        {
            int c;
            DotType dotType;
            sbyte dir;

            if (iter == null)
            {
                return false;
            }

            for (dir = -1; (c = iter(context, dir)) >= 0; dir = 0)
            {
                if (c == 0x49)
                {
                    return true; /* preceded by I */
                }
                dotType = GetDotType(c);
                if (dotType != DotType.OtherAccent)
                {
                    return false; /* preceded by different base character (not I), or intervening cc==230 */
                }
            }

            return false; /* not preceded by I */
        }

        /// <summary>Is followed by one or more cc==230 ?</summary>
        private bool IsFollowedByMoreAbove(UCaseContextIterator? iter, IntPtr context)
        {
            int c;
            DotType dotType;
            sbyte dir;

            if (iter == null)
            {
                return false;
            }

            for (dir = 1; (c = iter(context, dir)) >= 0; dir = 0)
            {
                dotType = GetDotType(c);
                if (dotType == DotType.Above)
                {
                    return true; /* at least one cc==230 following */
                }
                else if (dotType != DotType.OtherAccent)
                {
                    return false; /* next base character, no more cc==230 following */
                }
            }

            return false; /* no more cc==230 following */
        }

        /// <summary>Is followed by a dot above (without cc==230 in between) ?</summary>
        private bool IsFollowedByDotAbove(UCaseContextIterator? iter, IntPtr context)
        {
            int c;
            DotType dotType;
            sbyte dir;

            if (iter == null)
            {
                return false;
            }

            for (dir = 1; (c = iter(context, dir)) >= 0; dir = 0)
            {
                if (c == 0x307)
                {
                    return true;
                }
                dotType = GetDotType(c);
                if (dotType != DotType.OtherAccent)
                {
                    return false; /* next base character or cc==230 in between */
                }
            }

            return false; /* no dot above following */
        }

        private const string
                iDot = "i\u0307",
                jDot = "j\u0307",
                iOgonekDot = "\u012f\u0307",
                iDotGrave = "i\u0307\u0300",
                iDotAcute = "i\u0307\u0301",
                iDotTilde = "i\u0307\u0303";


        /// <summary>
        /// Get the full lowercase mapping for <paramref name="c"/>.
        /// </summary>
        /// <param name="c">Character to be mapped.</param>
        /// <param name="iter">
        /// Character iterator, used for context-sensitive mappings.
        /// See <see cref="UCaseContextIterator"/> for details.
        /// If iter==null then a context-independent result is returned.
        /// </param>
        /// <param name="context">Pointer to be passed into <paramref name="iter"/>.</param>
        /// <param name="output">If the mapping result is a string, then it is appended to <paramref name="output"/>.</param>
        /// <param name="caseLocale">Case locale value from <see cref="GetCaseLocale(System.Globalization.CultureInfo)"/>.</param>
        /// <returns>Output code point or string length, see <see cref="MaxStringLength"/>.</returns>
        /// <seealso cref="UCaseContextIterator"/>
        /// <seealso cref="MaxStringLength"/>
        /// <internal/>
        [CLSCompliant(false)]
        public int ToFullLower(int c, UCaseContextIterator? iter, IntPtr context, IAppendable output, CaseLocale caseLocale) // ICU4N TODO: API: Factor this out and use ValueStringBuilder to return a Span<T>
        {
            var sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                int result = ToFullLower(c, iter, context, ref sb, caseLocale);
                output.Append(sb.AsSpan());
                return result;
            }
            finally
            {
                sb.Dispose();
            }
        }

        /// <summary>
        /// Get the full lowercase mapping for <paramref name="c"/>.
        /// </summary>
        /// <param name="c">Character to be mapped.</param>
        /// <param name="iter">
        /// Character iterator, used for context-sensitive mappings.
        /// See <see cref="UCaseContextIterator"/> for details.
        /// If iter==null then a context-independent result is returned.
        /// </param>
        /// <param name="context">Pointer to be passed into <paramref name="iter"/>.</param>
        /// <param name="output">If the mapping result is a string, then it is appended to <paramref name="output"/>.</param>
        /// <param name="caseLocale">Case locale value from <see cref="GetCaseLocale(System.Globalization.CultureInfo)"/>.</param>
        /// <returns>Output code point or string length, see <see cref="MaxStringLength"/>.</returns>
        /// <seealso cref="UCaseContextIterator"/>
        /// <seealso cref="MaxStringLength"/>
        /// <internal/>
        [CLSCompliant(false)]
        public int ToFullLower(int c, UCaseContextIterator? iter, IntPtr context, StringBuilder output, CaseLocale caseLocale) // ICU4N TODO: API: Factor this out and use ValueStringBuilder to return a Span<T>
        {
            var sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                int result = ToFullLower(c, iter, context, ref sb, caseLocale);
                output.Append(sb.AsSpan());
                return result;
            }
            finally
            {
                sb.Dispose();
            }
        }

        /// <summary>
        /// Get the full lowercase mapping for <paramref name="c"/>.
        /// </summary>
        /// <param name="c">Character to be mapped.</param>
        /// <param name="iter">
        /// A delegate used for context-sensitive mappings.
        /// See <see cref="UCaseContextIterator"/> for details.
        /// If iter==<c>null</c> then a context-independent result is returned.
        /// </param>
        /// <param name="context">The context (surrounding text) to look at for conditional character mappings.</param>
        /// <param name="output">If the mapping result is a string, then it is appended to <paramref name="output"/>.</param>
        /// <param name="caseLocale">Case locale value from <see cref="GetCaseLocale(System.Globalization.CultureInfo)"/>.</param>
        /// <returns>Output code point or string length, see <see cref="MaxStringLength"/>.</returns>
        /// <seealso cref="UCaseContextIterator"/>
        /// <seealso cref="MaxStringLength"/>
        /// <internal/>
        internal int ToFullLower(int c, UCaseContextIterator? iter, IntPtr context, ref ValueStringBuilder output, CaseLocale caseLocale)
        {
            int result, props;

            result = c;
            props = trie.Get(c);
            if (!PropsHasException(props))
            {
                if (GetCaseTypeFromProps(props) >= CaseType.Upper)
                {
                    result = c + GetDelta(props);
                }
            }
            else
            {
                int excOffset = GetExceptionsOffset(props), excOffset2;
                int excWord = exceptions[excOffset++];
                int full;

                excOffset2 = excOffset;

                if ((excWord & EXC_CONDITIONAL_SPECIAL) != 0)
                {
                    /* use hardcoded conditions and mappings */
                    /*
                     * Test for conditional mappings first
                     *   (otherwise the unconditional default mappings are always taken),
                     * then test for characters that have unconditional mappings in SpecialCasing.txt,
                     * then get the UnicodeData.txt mappings.
                     */
                    if (caseLocale == CaseLocale.Lithuanian &&
                            /* base characters, find accents above */
                            (((c == 0x49 || c == 0x4a || c == 0x12e) &&
                                IsFollowedByMoreAbove(iter, context)) ||
                            /* precomposed with accent above, no need to find one */
                            (c == 0xcc || c == 0xcd || c == 0x128))
                        )
                    {
                        /*
                            # Lithuanian

                            # Lithuanian retains the dot in a lowercase i when followed by accents.

                            # Introduce an explicit dot above when lowercasing capital I's and J's
                            # whenever there are more accents above.
                            # (of the accents used in Lithuanian: grave, acute, tilde above, and ogonek)

                            0049; 0069 0307; 0049; 0049; lt More_Above; # LATIN CAPITAL LETTER I
                            004A; 006A 0307; 004A; 004A; lt More_Above; # LATIN CAPITAL LETTER J
                            012E; 012F 0307; 012E; 012E; lt More_Above; # LATIN CAPITAL LETTER I WITH OGONEK
                            00CC; 0069 0307 0300; 00CC; 00CC; lt; # LATIN CAPITAL LETTER I WITH GRAVE
                            00CD; 0069 0307 0301; 00CD; 00CD; lt; # LATIN CAPITAL LETTER I WITH ACUTE
                            0128; 0069 0307 0303; 0128; 0128; lt; # LATIN CAPITAL LETTER I WITH TILDE
                         */
                        // ICU4N: Removed unnecessary try/catch
                        switch (c)
                        {
                            case 0x49:  /* LATIN CAPITAL LETTER I */
                                output.Append(iDot);
                                return 2;
                            case 0x4a:  /* LATIN CAPITAL LETTER J */
                                output.Append(jDot);
                                return 2;
                            case 0x12e: /* LATIN CAPITAL LETTER I WITH OGONEK */
                                output.Append(iOgonekDot);
                                return 2;
                            case 0xcc:  /* LATIN CAPITAL LETTER I WITH GRAVE */
                                output.Append(iDotGrave);
                                return 3;
                            case 0xcd:  /* LATIN CAPITAL LETTER I WITH ACUTE */
                                output.Append(iDotAcute);
                                return 3;
                            case 0x128: /* LATIN CAPITAL LETTER I WITH TILDE */
                                output.Append(iDotTilde);
                                return 3;
                            default:
                                return 0; /* will not occur */
                        }
                        /* # Turkish and Azeri */
                    }
                    else if (caseLocale == CaseLocale.Turkish && c == 0x130)
                    {
                        /*
                            # I and i-dotless; I-dot and i are case pairs in Turkish and Azeri
                            # The following rules handle those cases.

                            0130; 0069; 0130; 0130; tr # LATIN CAPITAL LETTER I WITH DOT ABOVE
                            0130; 0069; 0130; 0130; az # LATIN CAPITAL LETTER I WITH DOT ABOVE
                         */
                        return 0x69;
                    }
                    else if (caseLocale == CaseLocale.Turkish && c == 0x307 && IsPrecededBy_I(iter, context))
                    {
                        /*
                            # When lowercasing, remove dot_above in the sequence I + dot_above, which will turn into i.
                            # This matches the behavior of the canonically equivalent I-dot_above

                            0307; ; 0307; 0307; tr After_I; # COMBINING DOT ABOVE
                            0307; ; 0307; 0307; az After_I; # COMBINING DOT ABOVE
                         */
                        return 0; /* remove the dot (continue without output) */
                    }
                    else if (caseLocale == CaseLocale.Turkish && c == 0x49 && !IsFollowedByDotAbove(iter, context))
                    {
                        /*
                            # When lowercasing, unless an I is before a dot_above, it turns into a dotless i.

                            0049; 0131; 0049; 0049; tr Not_Before_Dot; # LATIN CAPITAL LETTER I
                            0049; 0131; 0049; 0049; az Not_Before_Dot; # LATIN CAPITAL LETTER I
                         */
                        return 0x131;
                    }
                    else if (c == 0x130)
                    {
                        /*
                            # Preserve canonical equivalence for I with dot. Turkic is handled below.

                            0130; 0069 0307; 0130; 0130; # LATIN CAPITAL LETTER I WITH DOT ABOVE
                         */
                        // ICU4N: Removed unnecessary try/catch
                        output.Append(iDot);
                        return 2;
                    }
                    else if (c == 0x3a3 &&
                              !IsFollowedByCasedLetter(iter, context, dir: 1) &&
                              IsFollowedByCasedLetter(iter, context, dir: -1) /* -1=preceded */)
                    {
                        /* greek capital sigma maps depending on surrounding cased letters (see SpecialCasing.txt) */
                        /*
                            # Special case for final form of sigma

                            03A3; 03C2; 03A3; 03A3; Final_Sigma; # GREEK CAPITAL LETTER SIGMA
                         */
                        return 0x3c2; /* greek small final sigma */
                    }
                    else
                    {
                        /* no known conditional special case mapping, use a normal mapping */
                    }
                }
                else if (HasSlot(excWord, EXC_FULL_MAPPINGS))
                {
                    long value = GetSlotValueAndOffset(excWord, EXC_FULL_MAPPINGS, excOffset);
                    full = (int)value & FULL_LOWER;
                    if (full != 0)
                    {
                        /* start of full case mapping strings */
                        excOffset = (int)(value >> 32) + 1;

                        // ICU4N: removed unnecessary try/catch

                        // append the lowercase mapping
                        output.Append(exceptions.AsSpan(excOffset, full)); // ICU4N: (excOffset + full) - excOffset == full

                        /* return the string length */
                        return full;
                    }
                }

                if (HasSlot(excWord, EXC_LOWER))
                {
                    result = GetSlotValue(excWord, EXC_LOWER, excOffset2);
                }
            }

            return (result == c) ? ~result : result;
        }

        /* internal */
        private int ToUpperOrTitle(int c,
            UCaseContextIterator? iter, IntPtr context,
            ref ValueStringBuilder output,
            CaseLocale caseLocale,
            bool upperNotTitle)
        {
            int result;
            int props;

            result = c;
            props = trie.Get(c);
            if (!PropsHasException(props))
            {
                if (GetCaseTypeFromProps(props) == CaseType.Lower)
                {
                    result = c + GetDelta(props);
                }
            }
            else
            {
                int excOffset = GetExceptionsOffset(props), excOffset2;
                int excWord = exceptions[excOffset++];
                int full, index;

                excOffset2 = excOffset;

                if ((excWord & EXC_CONDITIONAL_SPECIAL) != 0)
                {
                    /* use hardcoded conditions and mappings */
                    if (caseLocale == CaseLocale.Turkish && c == 0x69)
                    {
                        /*
                            # Turkish and Azeri

                            # I and i-dotless; I-dot and i are case pairs in Turkish and Azeri
                            # The following rules handle those cases.

                            # When uppercasing, i turns into a dotted capital I

                            0069; 0069; 0130; 0130; tr; # LATIN SMALL LETTER I
                            0069; 0069; 0130; 0130; az; # LATIN SMALL LETTER I
                        */
                        return 0x130;
                    }
                    else if (caseLocale == CaseLocale.Lithuanian && c == 0x307 && IsPrecededBySoftDotted(iter, context))
                    {
                        /*
                            # Lithuanian

                            # Lithuanian retains the dot in a lowercase i when followed by accents.

                            # Remove DOT ABOVE after "i" with upper or titlecase

                            0307; 0307; ; ; lt After_Soft_Dotted; # COMBINING DOT ABOVE
                         */
                        return 0; /* remove the dot (continue without output) */
                    }
                    else
                    {
                        /* no known conditional special case mapping, use a normal mapping */
                    }
                }
                else if (HasSlot(excWord, EXC_FULL_MAPPINGS))
                {
                    long value = GetSlotValueAndOffset(excWord, EXC_FULL_MAPPINGS, excOffset);
                    full = (int)value & 0xffff;

                    /* start of full case mapping strings */
                    excOffset = (int)(value >> 32) + 1;

                    /* skip the lowercase and case-folding result strings */
                    excOffset += full & FULL_LOWER;
                    full >>= 4;
                    excOffset += full & 0xf;
                    full >>= 4;

                    if (upperNotTitle)
                    {
                        full &= 0xf;
                    }
                    else
                    {
                        /* skip the uppercase result string */
                        excOffset += full & 0xf;
                        full = (full >> 4) & 0xf;
                    }

                    if (full != 0)
                    {
                        // ICU4N: Removed unnecessary try/catch

                        // append the result string
                        output.Append(exceptions.AsSpan(excOffset, full)); // ICU4N: (excOffset + full) - excOffset == full

                        /* return the string length */
                        return full;
                    }
                }

                if (!upperNotTitle && HasSlot(excWord, EXC_TITLE))
                {
                    index = EXC_TITLE;
                }
                else if (HasSlot(excWord, EXC_UPPER))
                {
                    /* here, titlecase is same as uppercase */
                    index = EXC_UPPER;
                }
                else
                {
                    return ~c;
                }
                result = GetSlotValue(excWord, index, excOffset2);
            }

            return (result == c) ? ~result : result;
        }

        [CLSCompliant(false)]
        public int ToFullUpper(int c,
            UCaseContextIterator? iter, IntPtr context,
            IAppendable output,
            CaseLocale caseLocale) // ICU4N TODO: API: Factor this out and use ValueStringBuilder to return a Span<T>
        {
            var sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                int result = ToFullUpper(c, iter, context, ref sb, caseLocale);
                output.Append(sb.AsSpan());
                return result;
            }
            finally
            {
                sb.Dispose();
            }
        }

        [CLSCompliant(false)]
        public int ToFullUpper(int c,
            UCaseContextIterator? iter, IntPtr context,
            StringBuilder output,
            CaseLocale caseLocale) // ICU4N TODO: API: Factor this out and use ValueStringBuilder to return a Span<T>
        {
            var sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                int result = ToFullUpper(c, iter, context, ref sb, caseLocale);
                output.Append(sb.AsSpan());
                return result;
            }
            finally
            {
                sb.Dispose();
            }
        }

        internal int ToFullUpper(int c,
            UCaseContextIterator? iter, IntPtr context,
            ref ValueStringBuilder output,
            CaseLocale caseLocale)
        {
            return ToUpperOrTitle(c, iter, context, ref output, caseLocale, true);
        }

        [CLSCompliant(false)]
        public int ToFullTitle(int c,
            UCaseContextIterator? iter, IntPtr context,
            IAppendable output,
            CaseLocale caseLocale) // ICU4N TODO: API: Factor this out and use ValueStringBuilder to return a Span<T>
        {
            var sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                int result = ToFullTitle(c, iter, context, ref sb, caseLocale);
                output.Append(sb.AsSpan());
                return result;
            }
            finally
            {
                sb.Dispose();
            }
        }

        [CLSCompliant(false)]
        public int ToFullTitle(int c,
            UCaseContextIterator? iter, IntPtr context,
            StringBuilder output,
            CaseLocale caseLocale) // ICU4N TODO: API: Factor this out and use ValueStringBuilder to return a Span<T>
        {
            var sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                int result = ToFullTitle(c, iter, context, ref sb, caseLocale);
                output.Append(sb.AsSpan());
                return result;
            }
            finally
            {
                sb.Dispose();
            }
        }

        internal int ToFullTitle(int c,
            UCaseContextIterator? iter, IntPtr context,
            ref ValueStringBuilder output,
            CaseLocale caseLocale)
        {
            return ToUpperOrTitle(c, iter, context, ref output, caseLocale, false);
        }

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
        /// Currently only bit 0 for <see cref="UChar.FoldCaseExcludeSpecialI"/>.
        /// It is conceivable that at some point we might use one more bit for using uppercase sharp s.
        /// It is conceivable that at some point we might want the option to use only simple case foldings
        /// when operating on strings.
        /// </remarks>
        /// <internal/>
        private const int FOLD_CASE_OPTIONS_MASK = 7;

        /// <summary>Returns the simple case folding mapping for <paramref name="c"/>.</summary>
        public int Fold(int c, FoldCase options)
        {
            int props = trie.Get(c);
            if (!PropsHasException(props))
            {
                if (GetCaseTypeFromProps(props) >= CaseType.Upper)
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
                    if (((int)options & FOLD_CASE_OPTIONS_MASK) == (int)FoldCase.Default)
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

        // ICU4N specific - ToFullFolding(int c, IAppendable output, int options) moved to UCaseProps.generated.tt

        /* case folding ------------------------------------------------------------- */

        public int ToFullFolding(int c, IAppendable output, int options) // ICU4N TODO: API: Factor this out and use ValueStringBuilder to return a Span<T>
        {
            if (output is null)
                throw new ArgumentNullException(nameof(output));

            var sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                int result = ToFullFolding(c, ref sb, options);
                output.Append(sb.AsSpan());
                return result;
            }
            finally
            {
                sb.Dispose();
            }
        }

        public int ToFullFolding(int c, StringBuilder output, int options) // ICU4N TODO: API: Factor this out and use ValueStringBuilder to return a Span<T>
        {
            if (output is null)
                throw new ArgumentNullException(nameof(output));

            var sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                int result = ToFullFolding(c, ref sb, options);
                output.Append(sb.AsSpan());
                return result;
            }
            finally
            {
                sb.Dispose();
            }
        }

        // Issue for canonical caseless match (UAX #21):
        // Turkic casefolding (using "T" mappings in CaseFolding.txt) does not preserve
        // canonical equivalence, unlike default-option casefolding.
        // For example, I-grave and I + grave fold to strings that are not canonically
        // equivalent.
        // For more details, see the comment in unorm_compare() in unorm.cpp
        // and the intermediate prototype changes for Jitterbug 2021.
        // (For example, revision 1.104 of uchar.c and 1.4 of CaseFolding.txt.)
        // 
        // This did not get fixed because it appears that it is not possible to fix
        // it for uppercase and lowercase characters (I-grave vs. i-grave)
        // together in a way that they still fold to common result strings.


        internal int ToFullFolding(int c, ref ValueStringBuilder output, int options)
        {
            int result;
            int props;

            result = c;
            props = trie.Get(c);
            if (!PropsHasException(props))
            {
                if (GetCaseTypeFromProps(props) >= CaseType.Upper)
                {
                    result = c + GetDelta(props);
                }
            }
            else
            {
                int excOffset = GetExceptionsOffset(props), excOffset2;
                int excWord = exceptions[excOffset++];
                int full, index;

                excOffset2 = excOffset;

                if ((excWord & EXC_CONDITIONAL_FOLD) != 0)
                {
                    /* use hardcoded conditions and mappings */
                    if ((options & FOLD_CASE_OPTIONS_MASK) == UChar.FoldCaseDefault)
                    {
                        /* default mappings */
                        if (c == 0x49)
                        {
                            /* 0049; C; 0069; # LATIN CAPITAL LETTER I */
                            return 0x69;
                        }
                        else if (c == 0x130)
                        {
                            /* 0130; F; 0069 0307; # LATIN CAPITAL LETTER I WITH DOT ABOVE */

                            // ICU4N: Removed unnecessary try/catch
                            output.Append(iDot);
                            return 2;
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
                else if (HasSlot(excWord, EXC_FULL_MAPPINGS))
                {
                    long value = GetSlotValueAndOffset(excWord, EXC_FULL_MAPPINGS, excOffset);
                    full = (int)value & 0xffff;

                    /* start of full case mapping strings */
                    excOffset = (int)(value >> 32) + 1;

                    /* skip the lowercase result string */
                    excOffset += full & FULL_LOWER;
                    full = (full >> 4) & 0xf;

                    if (full != 0)
                    {
                        // ICU4N: Removed unnecessary try/catch

                        // append the result string
                        output.Append(exceptions, excOffset, full); // ICU4N: (excOffset + full) - excOffset == full

                        /* return the string length */
                        return full;
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
                    return ~c;
                }
                result = GetSlotValue(excWord, index, excOffset2);
            }

            return (result == c) ? ~result : result;
        }


        /* case mapping properties API ---------------------------------------------- */

        // ICU4N: Removed dummyStringBuilder because we use ValueStringBuilder, which does the
        // entire operation on the stack.

        public bool HasBinaryProperty(int c, UProperty which)
        {
            switch (which)
            {
                case UProperty.Lowercase:
                    return CaseType.Lower == GetCaseType(c);
                case UProperty.Uppercase:
                    return CaseType.Upper == GetCaseType(c);
                case UProperty.Soft_Dotted:
                    return IsSoftDotted(c);
                case UProperty.Case_Sensitive:
                    return IsCaseSensitive(c);
                case UProperty.Cased:
                    return CaseType.None != GetCaseType(c);
                case UProperty.Case_Ignorable:
                    // ICU4N: Simplfied version of GetTypeOrIgnorable
                    return IsCaseIgnorable(c, out CaseType _);
                //return (GetTypeOrIgnorable(c) >> 2) != 0;
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
                case UProperty.Changes_When_Lowercased:
                    return ChangesWhenLowercased(c);
                case UProperty.Changes_When_Uppercased:
                    return ChangesWhenUppercased(c);
                case UProperty.Changes_When_Titlecased:
                    return ChangesWhenTitlecased(c);
                /* case UProperty.CHANGES_WHEN_CASEFOLDED: -- in UCharacterProperty.java */
                case UProperty.Changes_When_Casemapped:
                    return ChangesWhenCasemapped(c);
                default:
                    return false;
            }
        }

        // ICU4N: Use helper methods to wrap ValueStringBuilder creation/destruction for these checks
        // so we don't have to do this for the other HasBinaryProperty() options.
        private bool ChangesWhenLowercased(int c)
        {
            var dummyStringBuilder = new ValueStringBuilder(stackalloc char[8]);
            try
            {
                return ToFullLower(c, null, IntPtr.Zero, ref dummyStringBuilder, CaseLocale.Root) >= 0;
            }
            finally
            {
                dummyStringBuilder.Dispose();
            }
        }

        private bool ChangesWhenUppercased(int c)
        {
            var dummyStringBuilder = new ValueStringBuilder(stackalloc char[8]);
            try
            {
                return ToFullUpper(c, null, IntPtr.Zero, ref dummyStringBuilder, CaseLocale.Root) >= 0;
            }
            finally
            {
                dummyStringBuilder.Dispose();
            }
        }

        private bool ChangesWhenTitlecased(int c)
        {
            var dummyStringBuilder = new ValueStringBuilder(stackalloc char[8]);
            try
            {
                return ToFullTitle(c, null, IntPtr.Zero, ref dummyStringBuilder, CaseLocale.Root) >= 0;
            }
            finally
            {
                dummyStringBuilder.Dispose();
            }
        }

        private bool ChangesWhenCasemapped(int c)
        {
            var dummyStringBuilder = new ValueStringBuilder(stackalloc char[8]);
            try
            {
                if (ToFullLower(c, null, IntPtr.Zero, ref dummyStringBuilder, CaseLocale.Root) >= 0) return true;
                dummyStringBuilder.Length = 0;
                if (ToFullUpper(c, null, IntPtr.Zero, ref dummyStringBuilder, CaseLocale.Root) >= 0) return true;
                dummyStringBuilder.Length = 0;
                return ToFullTitle(c, null, IntPtr.Zero, ref dummyStringBuilder, CaseLocale.Root) >= 0;
            }
            finally
            {
                dummyStringBuilder.Dispose();
            }
        }

#nullable restore

        // data members -------------------------------------------------------- ***
        private int[] indexes;
        private string exceptions;
        private char[] unfold;

        private Trie2_16 trie;

        // data format constants ----------------------------------------------- ***
        private const string DATA_NAME = "ucase";
        private const string DATA_TYPE = "icu";
        private const string DATA_FILE_NAME = DATA_NAME + "." + DATA_TYPE;

        /// <summary>format "cAsE"</summary>
        private const int FMT = 0x63415345;

        /* indexes into indexes[] */
        //private const int IX_INDEX_TOP=0;
        //private const int IX_LENGTH=1;
        private const int IX_TRIE_SIZE = 2;
        private const int IX_EXC_LENGTH = 3;
        private const int IX_UNFOLD_LENGTH = 4;

        //private const int IX_MAX_FULL_LENGTH=15;
        private const int IX_TOP = 16;

        // definitions for 16-bit case properties word ------------------------- ***

        /* 2-bit constants for types of cased characters */
        private const int TypeMask = 3;
        // ICU4N specific: Refactored Type constants into an enum named CaseType

        /// <returns><see cref="CaseType.None"/>, <see cref="CaseType.Lower"/>, <see cref="CaseType.Upper"/>, <see cref="CaseType.Title"/></returns>
        private static CaseType GetCaseTypeFromProps(int props)
        {
            return (CaseType)(props & TypeMask);
        }

        /// <summary>
        /// Like <see cref="GetCaseTypeFromProps(int)"/>, but returns <c>true</c> if <paramref name="props"/> indicate case-ignorable
        /// and returns <paramref name="type"/> as an out parameter.
        /// </summary>
        // ICU4N specific - rather than returning 2 bits in an int in GetTypeAndIgnorableFromProps, 
        // we return ignorable as a bool and type as an out parameter, which makes usage simpler for end users
        private static bool IsCaseIgnorableFromProps(int props, out CaseType type)
        {
            type = (CaseType)(props & TypeMask);
            return (props & IGNORABLE) != 0; // Return true if ignorable
        }

        internal const int IGNORABLE = 4;
        private const int SENSITIVE = 8;
        private const int EXCEPTION = 0x10;

        private const int DOT_MASK = 0x60;
        // ICU4N: moved constants to DotType enumeration

        /* no exception: bits 15..7 are a 9-bit signed case mapping delta */
        private const int DELTA_SHIFT = 7;
        //private const int DELTA_MASK=    0xff80;
        //private const int MAX_DELTA=     0xff;
        //private const int MIN_DELTA=     (-MAX_DELTA-1);

        private static int GetDelta(int props)
        {
            return (short)props >> DELTA_SHIFT;
        }

        /// <summary>exception: bits 15..5 are an unsigned 11-bit index into the exceptions array</summary>
        private const int EXC_SHIFT = 5;
        //private const int EXC_MASK=      0xffe0;
        //private const int MAX_EXCEPTIONS=((EXC_MASK>>EXC_SHIFT)+1);

        /* definitions for 16-bit main exceptions word ------------------------------ */

        /// <summary>first 8 bits indicate values in optional slots</summary>
        private const int EXC_LOWER = 0;
        private const int EXC_FOLD = 1;
        private const int EXC_UPPER = 2;
        private const int EXC_TITLE = 3;
        //private const int EXC_4=4;           /* reserved */
        //private const int EXC_5=5;           /* reserved */
        private const int EXC_CLOSURE = 6;
        private const int EXC_FULL_MAPPINGS = 7;
        //private const int EXC_ALL_SLOTS=8;   /* one past the last slot */

        /// <summary>each slot is 2 uint16_t instead of 1</summary>
        private const int EXC_DOUBLE_SLOTS = 0x100;

        /* reserved: exception bits 11..9 */

        /// <summary>EXC_DOT_MASK=<see cref="DOT_MASK"/>&lt;&lt;<see cref="EXC_DOT_SHIFT"/></summary>
        private const int EXC_DOT_SHIFT = 7;

        /* normally stored in the main word, but pushed out for larger exception indexes */
        //private const int EXC_DOT_MASK=              0x3000;
        //private const int EXC_NO_DOT=                0;
        //private const int EXC_SOFT_DOTTED=           0x1000;
        //private const int EXC_ABOVE=                 0x2000; /* "above" accents with cc=230 */
        //private const int EXC_OTHER_ACCENT=          0x3000; /* other character (0<cc!=230) */

        /* complex/conditional mappings */
        private const int EXC_CONDITIONAL_SPECIAL = 0x4000;
        private const int EXC_CONDITIONAL_FOLD = 0x8000;

        /* definitions for lengths word for full case mappings */
        private const int FULL_LOWER = 0xf;
        //private const int FULL_FOLDING=  0xf0;
        //private const int FULL_UPPER=    0xf00;
        //private const int FULL_TITLE=    0xf000;

        /* maximum lengths */
        //private const int FULL_MAPPINGS_MAX_LENGTH=4*0xf;
        private const int CLOSURE_MAX_LENGTH = 0xf;

        /* constants for reverse case folding ("unfold") data */
        private const int UNFOLD_ROWS = 0;
        private const int UNFOLD_ROW_WIDTH = 1;
        private const int UNFOLD_STRING_WIDTH = 2;

        /// <summary>
        /// Public singleton instance.
        /// </summary>
        public static UCaseProperties Instance { get; private set; } = LoadSingletonInstance();

        // ICU4N: Avoid static constructors
        private static UCaseProperties LoadSingletonInstance()
        {
            try
            {
                return new UCaseProperties();
            }
            catch (IOException e)
            {
                throw new ICUUncheckedIOException(e);
            }
        }
    }
}
