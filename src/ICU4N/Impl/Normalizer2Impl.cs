using ICU4N.Support.Text;
using ICU4N.Text;
using ICU4N.Util;
using J2N;
using J2N.IO;
using J2N.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ICU4N.Impl
{
    public sealed partial class Hangul
    {
        /* Korean Hangul and Jamo constants */
        public const int JamoLBase = 0x1100;     /* "lead" jamo */
        public const int JamoLEnd = 0x1112;
        public const int JamoVBase = 0x1161;     /* "vowel" jamo */
        public const int JamoVEnd = 0x1175;
        public const int JamoTBase = 0x11a7;     /* "trail" jamo */
        public const int JamoTEnd = 0x11c2;

        public const int HangulBase = 0xac00;
        public const int HangulEnd = 0xd7a3;

        public const int JamoLCount = 19;
        public const int JamoVCount = 21;
        public const int JamoTCount = 28;

        public const int JamoLLimit = JamoLBase + JamoLCount;
        public const int JamoVLimit = JamoVBase + JamoVCount;

        public const int JamoVTCount = JamoVCount * JamoTCount;

        public const int HangulCount = JamoLCount * JamoVCount * JamoTCount;
        public const int HangulLimit = HangulBase + HangulCount;

        public static bool IsHangul(int c)
        {
            return HangulBase <= c && c < HangulLimit;
        }
        public static bool IsHangulLV(int c)
        {
            c -= HangulBase;
            return 0 <= c && c < HangulCount && c % JamoTCount == 0;
        }
        public static bool IsJamoL(int c)
        {
            return JamoLBase <= c && c < JamoLLimit;
        }
        public static bool IsJamoV(int c)
        {
            return JamoVBase <= c && c < JamoVLimit;
        }
        public static bool IsJamoT(int c)
        {
            int t = c - JamoTBase;
            return 0 < t && t < JamoTCount;  // not JamoTBase itself
        }
        public static bool IsJamo(int c)
        {
            return JamoLBase <= c && c <= JamoTEnd &&
                (c <= JamoLEnd || (JamoVBase <= c && c <= JamoVEnd) || JamoTBase < c);
        }

        // ICU4N specific - Decompose(int c, IAppendable buffer) moved to Normalizer2ImplExtension.tt

        // ICU4N specific - GetRawDecomposition(int c, IAppendable buffer) moved to Normalizer2ImplExtension.tt
    }

    /// <summary>
    /// Writable buffer that takes care of canonical ordering.
    /// Its <see cref="IAppendable"/> methods behave like the C++ implementation's
    /// appendZeroCC() methods.
    /// <para/>
    /// If dest is a <see cref="System.Text.StringBuilder"/>, then the buffer writes directly to it.
    /// Otherwise, the buffer maintains a <see cref="System.Text.StringBuilder"/> for intermediate text segments
    /// until no further changes are necessary and whole segments are appended.
    /// Append() methods that take combining-class values always write to the <see cref="System.Text.StringBuilder"/>.
    /// Other Append() methods flush and append to the <see cref="IAppendable"/>.
    /// </summary>
    public sealed partial class ReorderingBuffer : IAppendable
    {
        public ReorderingBuffer(Normalizer2Impl ni, StringBuilder dest, int destCapacity)
            : this(ni, dest.ToAppendable(), destCapacity)
        {
        }

        internal ReorderingBuffer(Normalizer2Impl ni, IAppendable dest, int destCapacity)
        {
            impl = ni;
            app = dest;
            if (app is StringBuilderCharSequence)
            {
                appIsStringBuilder = true;
                str = ((StringBuilderCharSequence)dest).Value;
                // In Java, the constructor subsumes public void init(int destCapacity) {
                str.EnsureCapacity(destCapacity);
                reorderStart = 0;
                if (str.Length == 0)
                {
                    lastCC = 0;
                }
                else
                {
                    SetIterator();
                    lastCC = PreviousCC();
                    // Set reorderStart after the last code point with cc<=1 if there is one.
                    if (lastCC > 1)
                    {
                        while (PreviousCC() > 1) { }
                    }
                    reorderStart = codePointLimit;
                }
            }
            else
            {
                appIsStringBuilder = false;
                str = new StringBuilder();
                reorderStart = 0;
                lastCC = 0;
            }
        }

        public bool IsEmpty { get { return str.Length == 0; } }
        public int Length { get { return str.Length; } }
        public int LastCC { get { return lastCC; } }

        public StringBuilder StringBuilder { get { return str; } }

        // ICU4N specific - Equals(ICharSequence s, int start, int limit) moved to Normalizer2ImplExtension.tt

        public void Append(int c, int cc)
        {
            if (lastCC <= cc || cc == 0)
            {
                str.AppendCodePoint(c);
                lastCC = cc;
                if (cc <= 1)
                {
                    reorderStart = str.Length;
                }
            }
            else
            {
                Insert(c, cc);
            }
        }

        // ICU4N specific - Append(ICharSequence s, int start, int limit,
        //    int leadCC, int trailCC)


        // The following append() methods work like C++ appendZeroCC().
        // They assume that the cc or trailCC of their input is 0.
        // Most of them implement Appendable interface methods.
        public ReorderingBuffer Append(char c)
        {
            str.Append(c);
            lastCC = 0;
            reorderStart = str.Length;
            return this;
        }
        public void AppendZeroCC(int c)
        {
            str.AppendCodePoint(c);
            lastCC = 0;
            reorderStart = str.Length;
        }

        // ICU4N specific - Append(ICharSequence s) moved to Normalizer2ImplExtension.tt

        // ICU4N specific - Append(ICharSequence s, int start, int limit)

        /// <summary>
        /// Flushes from the intermediate <see cref="StringBuilder"/> to the <see cref="IAppendable"/>,
        /// if they are different objects.
        /// Used after recomposition.
        /// Must be called at the end when writing to a non-<see cref="StringBuilderCharSequence"/> <see cref="IAppendable"/>.
        /// </summary>
        public void Flush()
        {
            if (appIsStringBuilder)
            {
                reorderStart = str.Length;
            }
            else
            {
                try
                {
                    app.Append(str);
                    str.Length = 0;
                    reorderStart = 0;
                }
                catch (IOException e)
                {
                    throw new ICUUncheckedIOException(e);  // Avoid declaring "throws IOException".
                }
            }
            lastCC = 0;
        }

        public void Remove()
        {
            str.Length = 0;
            lastCC = 0;
            reorderStart = 0;
        }
        public void RemoveSuffix(int suffixLength)
        {
            int oldLength = str.Length;
            str.Delete(oldLength - suffixLength, suffixLength); // ICU4N: Corrected 2nd parameter
            lastCC = 0;
            reorderStart = str.Length;
        }

        // ICU4N specific - FlushAndAppendZeroCC(ICharSequence s, int start, int limit)

        /*
         * TODO: Revisit whether it makes sense to track reorderStart.
         * It is set to after the last known character with cc<=1,
         * which stops previousCC() before it reads that character and looks up its cc.
         * previousCC() is normally only called from insert().
         * In other words, reorderStart speeds up the insertion of a combining mark
         * into a multi-combining mark sequence where it does not belong at the end.
         * This might not be worth the trouble.
         * On the other hand, it's not a huge amount of trouble.
         *
         * We probably need it for UNORM_SIMPLE_APPEND.
         */

        // Inserts c somewhere before the last character.
        // Requires 0<cc<lastCC which implies reorderStart<limit.
        private void Insert(int c, int cc)
        {
            for (SetIterator(), SkipPrevious(); PreviousCC() > cc;) { }
            // insert c at codePointLimit, after the character with prevCC<=cc
            if (c <= 0xffff)
            {
                str.Insert(codePointLimit, (char)c);
                if (cc <= 1)
                {
                    reorderStart = codePointLimit + 1;
                }
            }
            else
            {
                str.Insert(codePointLimit, Character.ToChars(c));
                if (cc <= 1)
                {
                    reorderStart = codePointLimit + 2;
                }
            }
        }

        private readonly Normalizer2Impl impl;
        private readonly IAppendable app;
        private readonly StringBuilder str;
        private readonly bool appIsStringBuilder;
        private int reorderStart;
        private int lastCC;

        // private backward iterator
        private void SetIterator() { codePointStart = str.Length; }
        private void SkipPrevious()
        {  // Requires 0<codePointStart.
            codePointLimit = codePointStart;
            codePointStart = str.OffsetByCodePoints(codePointStart, -1);
        }
        private int PreviousCC()
        {  // Returns 0 if there is no previous character.
            codePointLimit = codePointStart;
            if (reorderStart >= codePointStart)
            {
                return 0;
            }
            int c = str.CodePointBefore(codePointStart);
            codePointStart -= Character.CharCount(c);
            return impl.GetCCFromYesOrMaybeCP(c);
        }

        // ICU4N specific - implementing interface explicitly allows
        // for us to have a concrete type above that returns itself (similar to
        // how it was in Java).
        #region IAppendable interface

        IAppendable IAppendable.Append(char c)
        {
            return Append(c);
        }

        IAppendable IAppendable.Append(string csq)
        {
            return Append(csq);
        }

        IAppendable IAppendable.Append(string csq, int start, int end)
        {
            return Append(csq, start, end);
        }

        IAppendable IAppendable.Append(StringBuilder csq)
        {
            return Append(csq);
        }

        IAppendable IAppendable.Append(StringBuilder csq, int start, int end)
        {
            return Append(csq, start, end);
        }

        IAppendable IAppendable.Append(char[] csq)
        {
            return Append(csq);
        }

        IAppendable IAppendable.Append(char[] csq, int start, int end)
        {
            return Append(csq, start, end);
        }

        IAppendable IAppendable.Append(ICharSequence csq)
        {
            return Append(csq);
        }

        IAppendable IAppendable.Append(ICharSequence csq, int start, int end)
        {
            return Append(csq, start, end);
        }

        #endregion

        private int codePointStart, codePointLimit;
    }

    // TODO: Propose as public API on the UTF16 class.
    // TODO: Propose widening UTF16 methods that take char to take int.
    // TODO: Propose widening UTF16 methods that take String to take CharSequence.
    public sealed partial class UTF16Plus
    {
        /// <summary>
        /// Assuming <paramref name="c"/> is a surrogate code point (UTF16.IsSurrogate(c)),
        /// is it a lead surrogate?
        /// </summary>
        /// <param name="c">code unit or code point</param>
        /// <returns>true or false</returns>
        public static bool IsSurrogateLead(int c) { return (c & 0x400) == 0; }

        // ICU4N specific -  Equal(ICharSequence s1, ICharSequence s2) moved to Normalizer2ImplExtension.tt

        // ICU4N specific -  Equal(ICharSequence s1, int start1, int limit1,
        //    ICharSequence s2, int start2, int limit2) moved to Normalizer2ImplExtension.tt
    }

    /// <summary>
    /// Low-level implementation of the Unicode Normalization Algorithm.
    /// For the data structure and details see the documentation at the end of
    /// C++ normalizer2impl.h and in the design doc at
    /// http://site.icu-project.org/design/normalization/custom
    /// </summary>
    public sealed partial class Normalizer2Impl
    {
        // ICU4N specific - de-nested Hangul class
        
        // ICU4N specific - de-nested ReorderingBuffer class
        
        // ICU4N specific - de-nested UTF16Plus class

        public Normalizer2Impl() { }

        private sealed class IsAcceptable : IAuthenticate
        {
            public bool IsDataVersionAcceptable(byte[] version)
            {
                return version[0] == 3;
            }
        }
        private static readonly IsAcceptable IS_ACCEPTABLE = new IsAcceptable();
        private const int DATA_FORMAT = 0x4e726d32;  // "Nrm2"

        public Normalizer2Impl Load(ByteBuffer bytes)
        {
            try
            {
                dataVersion = ICUBinary.ReadHeaderAndDataVersion(bytes, DATA_FORMAT, IS_ACCEPTABLE);
                int indexesLength = bytes.GetInt32() / 4;  // inIndexes[IX_NORM_TRIE_OFFSET]/4
                if (indexesLength <= IX_MIN_LCCC_CP)
                {
                    throw new ICUUncheckedIOException("Normalizer2 data: not enough indexes");
                }
                int[] inIndexes = new int[indexesLength];
                inIndexes[0] = indexesLength * 4;
                for (int i = 1; i < indexesLength; ++i)
                {
                    inIndexes[i] = bytes.GetInt32();
                }

                minDecompNoCP = inIndexes[IX_MIN_DECOMP_NO_CP];
                minCompNoMaybeCP = inIndexes[IX_MIN_COMP_NO_MAYBE_CP];
                minLcccCP = inIndexes[IX_MIN_LCCC_CP];

                minYesNo = inIndexes[IX_MIN_YES_NO];
                minYesNoMappingsOnly = inIndexes[IX_MIN_YES_NO_MAPPINGS_ONLY];
                minNoNo = inIndexes[IX_MIN_NO_NO];
                minNoNoCompBoundaryBefore = inIndexes[IX_MIN_NO_NO_COMP_BOUNDARY_BEFORE];
                minNoNoCompNoMaybeCC = inIndexes[IX_MIN_NO_NO_COMP_NO_MAYBE_CC];
                minNoNoEmpty = inIndexes[IX_MIN_NO_NO_EMPTY];
                limitNoNo = inIndexes[IX_LIMIT_NO_NO];
                minMaybeYes = inIndexes[IX_MIN_MAYBE_YES];
                Debug.Assert((minMaybeYes & 7) == 0);  // 8-aligned for noNoDelta bit fields
                centerNoNoDelta = (minMaybeYes >> DELTA_SHIFT) - MAX_DELTA - 1;

                // Read the normTrie.
                int offset = inIndexes[IX_NORM_TRIE_OFFSET];
                int nextOffset = inIndexes[IX_EXTRA_DATA_OFFSET];
                normTrie = Trie2_16.CreateFromSerialized(bytes);
                int trieLength = normTrie.SerializedLength;
                if (trieLength > (nextOffset - offset))
                {
                    throw new ICUUncheckedIOException("Normalizer2 data: not enough bytes for normTrie");
                }
                ICUBinary.SkipBytes(bytes, (nextOffset - offset) - trieLength);  // skip padding after trie bytes

                // Read the composition and mapping data.
                offset = nextOffset;
                nextOffset = inIndexes[IX_SMALL_FCD_OFFSET];
                int numChars = (nextOffset - offset) / 2;
                if (numChars != 0)
                {
                    maybeYesCompositions = ICUBinary.GetString(bytes, numChars, 0);
                    extraData = maybeYesCompositions.Substring((MIN_NORMAL_MAYBE_YES - minMaybeYes) >> OFFSET_SHIFT);
                }

                // smallFCD: new in formatVersion 2
                offset = nextOffset;
                smallFCD = new byte[0x100];
                bytes.Get(smallFCD);

                return this;
            }
            catch (IOException e)
            {
                throw new ICUUncheckedIOException(e);
            }
        }
        public Normalizer2Impl Load(string name)
        {
            var data = ICUBinary.GetRequiredData(name);
            return Load(data);
        }

        private void EnumLcccRange(int start, int end, int norm16, UnicodeSet set)
        {
            if (norm16 > MIN_NORMAL_MAYBE_YES && norm16 != JAMO_VT)
            {
                set.Add(start, end);
            }
            else if (minNoNoCompNoMaybeCC <= norm16 && norm16 < limitNoNo)
            {
                int fcd16 = GetFCD16(start);
                if (fcd16 > 0xff) { set.Add(start, end); }
            }
        }

        private void EnumNorm16PropertyStartsRange(int start, int end, int value, UnicodeSet set)
        {
            /* add the start code point to the USet */
            set.Add(start);
            if (start != end && IsAlgorithmicNoNo(value) && (value & DELTA_TCCC_MASK) > DELTA_TCCC_1)
            {
                // Range of code points with same-norm16-value algorithmic decompositions.
                // They might have different non-zero FCD16 values.
                int prevFCD16 = GetFCD16(start);
                while (++start <= end)
                {
                    int fcd16 = GetFCD16(start);
                    if (fcd16 != prevFCD16)
                    {
                        set.Add(start);
                        prevFCD16 = fcd16;
                    }
                }
            }
        }

        public void AddLcccChars(UnicodeSet set)
        {
            using (var trieIterator = normTrie.GetEnumerator())
            {
                Trie2Range range;
                while (trieIterator.MoveNext() && !(range = trieIterator.Current).IsLeadSurrogate)
                {
                    EnumLcccRange(range.StartCodePoint, range.EndCodePoint, range.Value, set);
                }
            }
        }

        public void AddPropertyStarts(UnicodeSet set)
        {
            /* add the start code point of each same-value range of each trie */
            using (var trieIterator = normTrie.GetEnumerator())
            {
                Trie2Range range;
                while (trieIterator.MoveNext() && !(range = trieIterator.Current).IsLeadSurrogate)
                {
                    EnumNorm16PropertyStartsRange(range.StartCodePoint, range.EndCodePoint, range.Value, set);
                }
            }

            /* add Hangul LV syllables and LV+1 because of skippables */
            for (int c = Hangul.HangulBase; c < Hangul.HangulLimit; c += Hangul.JamoTCount)
            {
                set.Add(c);
                set.Add(c + 1);
            }
            set.Add(Hangul.HangulLimit); /* add Hangul+1 to continue with other properties */
        }

        public void AddCanonIterPropertyStarts(UnicodeSet set)
        {
            /* add the start code point of each same-value range of the canonical iterator data trie */
            EnsureCanonIterData();
            // currently only used for the SEGMENT_STARTER property
            using (var trieIterator = canonIterData.GetEnumerator(segmentStarterMapper))
            {
                Trie2Range range;
                while (trieIterator.MoveNext() && !(range = trieIterator.Current).IsLeadSurrogate)
                {
                    /* add the start code point to the USet */
                    set.Add(range.StartCodePoint);
                }
            }
        }

        private class SegmentValueMapper : IValueMapper
        {
            public int Map(int input)
            {
                return (int)(input & CANON_NOT_SEGMENT_STARTER);
            }
        }


        private static readonly IValueMapper segmentStarterMapper = new SegmentValueMapper();


        // low-level properties ------------------------------------------------ ***

        // Note: Normalizer2Impl.java r30983 (2011-nov-27)
        // still had getFCDTrie() which built and cached an FCD trie.
        // That provided faster access to FCD data than getFCD16FromNormData()
        // but required synchronization and consumed some 10kB of heap memory
        // in any process that uses FCD (e.g., via collation).
        // minDecompNoCP etc. and smallFCD[] are intended to help with any loss of performance,
        // at least for ASCII & CJK.

        /// <summary>
        /// Builds the canonical-iterator data for this instance.
        /// This is required before any of <see cref="IsCanonSegmentStarter(int)"/> or
        /// <see cref="GetCanonStartSet(int, UnicodeSet)"/> are called,
        /// or else they crash.
        /// </summary>
        /// <returns>This.</returns>
        public Normalizer2Impl EnsureCanonIterData()
        {
            lock (this)
            {
                if (canonIterData == null)
                {
                    Trie2Writable newData = new Trie2Writable(0, 0);
                    canonStartSets = new List<UnicodeSet>();
                    using (var trieIterator = normTrie.GetEnumerator())
                    {
                        Trie2Range range;
                        while (trieIterator.MoveNext() && !(range = trieIterator.Current).IsLeadSurrogate)
                        {
                            int norm16 = range.Value;
                            if (IsInert(norm16) || (minYesNo <= norm16 && norm16 < minNoNo))
                            {
                                // Inert, or 2-way mapping (including Hangul syllable).
                                // We do not write a canonStartSet for any yesNo character.
                                // Composites from 2-way mappings are added at runtime from the
                                // starter's compositions list, and the other characters in
                                // 2-way mappings get CANON_NOT_SEGMENT_STARTER set because they are
                                // "maybe" characters.
                                continue;
                            }
                            for (int c = range.StartCodePoint; c <= range.EndCodePoint; ++c)
                            {
                                int oldValue = newData.Get(c);
                                int newValue = oldValue;
                                if (IsMaybeOrNonZeroCC(norm16))
                                {
                                    // not a segment starter if it occurs in a decomposition or has cc!=0
                                    newValue |= (int)CANON_NOT_SEGMENT_STARTER;
                                    if (norm16 < MIN_NORMAL_MAYBE_YES)
                                    {
                                        newValue |= CANON_HAS_COMPOSITIONS;
                                    }
                                }
                                else if (norm16 < minYesNo)
                                {
                                    newValue |= CANON_HAS_COMPOSITIONS;
                                }
                                else
                                {
                                    // c has a one-way decomposition
                                    int c2 = c;
                                    // Do not modify the whole-range norm16 value.
                                    int norm16_2 = norm16;
                                    if (IsDecompNoAlgorithmic(norm16_2))
                                    {
                                        // Maps to an isCompYesAndZeroCC.
                                        c2 = MapAlgorithmic(c2, norm16_2);
                                        norm16_2 = GetNorm16(c2);
                                        // No compatibility mappings for the CanonicalIterator.
                                        Debug.Assert(!(IsHangulLV(norm16_2) || IsHangulLVT(norm16_2)));
                                    }
                                    if (norm16_2 > minYesNo)
                                    {
                                        // c decomposes, get everything from the variable-length extra data
                                        int mapping = norm16_2 >> OFFSET_SHIFT;
                                        int firstUnit = extraData[mapping];
                                        int length = firstUnit & MAPPING_LENGTH_MASK;
                                        if ((firstUnit & MAPPING_HAS_CCC_LCCC_WORD) != 0)
                                        {
                                            if (c == c2 && (extraData[mapping - 1] & 0xff) != 0)
                                            {
                                                newValue |= (int)CANON_NOT_SEGMENT_STARTER;  // original c has cc!=0
                                            }
                                        }
                                        // Skip empty mappings (no characters in the decomposition).
                                        if (length != 0)
                                        {
                                            ++mapping;  // skip over the firstUnit
                                                        // add c to first code point's start set
                                            int limit = mapping + length;
                                            c2 = extraData.CodePointAt(mapping);
                                            AddToStartSet(newData, c, c2);
                                            // Set CANON_NOT_SEGMENT_STARTER for each remaining code point of a
                                            // one-way mapping. A 2-way mapping is possible here after
                                            // intermediate algorithmic mapping.
                                            if (norm16_2 >= minNoNo)
                                            {
                                                while ((mapping += Character.CharCount(c2)) < limit)
                                                {
                                                    c2 = extraData.CodePointAt(mapping);
                                                    int c2Value = newData.Get(c2);
                                                    if ((c2Value & CANON_NOT_SEGMENT_STARTER) == 0)
                                                    {
                                                        newData.Set(c2, c2Value | (int)CANON_NOT_SEGMENT_STARTER);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // c decomposed to c2 algorithmically; c has cc==0
                                        AddToStartSet(newData, c, c2);
                                    }
                                }
                                if (newValue != oldValue)
                                {
                                    newData.Set(c, newValue);
                                }
                            }
                        }
                    }
                    canonIterData = newData.ToTrie2_32();
                }
                return this;
            }
        }

        public int GetNorm16(int c) { return normTrie.Get(c); }

        public int GetCompQuickCheck(int norm16)
        {
            if (norm16 < minNoNo || MIN_YES_YES_WITH_CC <= norm16)
            {
                return 1;  // yes
            }
            else if (minMaybeYes <= norm16)
            {
                return 2;  // maybe
            }
            else
            {
                return 0;  // no
            }
        }
        public bool IsAlgorithmicNoNo(int norm16) { return limitNoNo <= norm16 && norm16 < minMaybeYes; }
        public bool IsCompNo(int norm16) { return minNoNo <= norm16 && norm16 < minMaybeYes; }
        public bool IsDecompYes(int norm16) { return norm16 < minYesNo || minMaybeYes <= norm16; }

        public int GetCC(int norm16)
        {
            if (norm16 >= MIN_NORMAL_MAYBE_YES)
            {
                return GetCCFromNormalYesOrMaybe(norm16);
            }
            if (norm16 < minNoNo || limitNoNo <= norm16)
            {
                return 0;
            }
            return GetCCFromNoNo(norm16);
        }
        public static int GetCCFromNormalYesOrMaybe(int norm16)
        {
            return (norm16 >> OFFSET_SHIFT) & 0xff;
        }
        public static int GetCCFromYesOrMaybe(int norm16)
        {
            return norm16 >= MIN_NORMAL_MAYBE_YES ? GetCCFromNormalYesOrMaybe(norm16) : 0;
        }
        public int GetCCFromYesOrMaybeCP(int c)
        {
            if (c < minCompNoMaybeCP) { return 0; }
            return GetCCFromYesOrMaybe(GetNorm16(c));
        }

        /// <summary>
        /// Returns the FCD data for code point <paramref name="c"/>.
        /// </summary>
        /// <param name="c">A Unicode code point.</param>
        /// <returns>The lccc(c) in bits 15..8 and tccc(c) in bits 7..0.</returns>
        public int GetFCD16(int c)
        {
            if (c < minDecompNoCP)
            {
                return 0;
            }
            else if (c <= 0xffff)
            {
                if (!SingleLeadMightHaveNonZeroFCD16(c)) { return 0; }
            }
            return GetFCD16FromNormData(c);
        }
        /// <summary>Returns true if the single-or-lead code unit c might have non-zero FCD data.</summary>
        public bool SingleLeadMightHaveNonZeroFCD16(int lead)
        {
            // 0<=lead<=0xffff
            byte bits = smallFCD[lead >> 8];
            if (bits == 0) { return false; }
            return ((bits >> ((lead >> 5) & 7)) & 1) != 0;
        }

        /// <summary>Gets the FCD value from the regular normalization data.</summary>
        public int GetFCD16FromNormData(int c)
        {
            int norm16 = GetNorm16(c);
            if (norm16 >= limitNoNo)
            {
                if (norm16 >= MIN_NORMAL_MAYBE_YES)
                {
                    // combining mark
                    norm16 = GetCCFromNormalYesOrMaybe(norm16);
                    return norm16 | (norm16 << 8);
                }
                else if (norm16 >= minMaybeYes)
                {
                    return 0;
                }
                else
                {  // isDecompNoAlgorithmic(norm16)
                    int deltaTrailCC = norm16 & DELTA_TCCC_MASK;
                    if (deltaTrailCC <= DELTA_TCCC_1)
                    {
                        return deltaTrailCC >> OFFSET_SHIFT;
                    }
                    // Maps to an isCompYesAndZeroCC.
                    c = MapAlgorithmic(c, norm16);
                    norm16 = GetNorm16(c);
                }
            }
            if (norm16 <= minYesNo || IsHangulLVT(norm16))
            {
                // no decomposition or Hangul syllable, all zeros
                return 0;
            }
            // c decomposes, get everything from the variable-length extra data
            int mapping = norm16 >> OFFSET_SHIFT;
            int firstUnit = extraData[mapping];
            int fcd16 = firstUnit >> 8;  // tccc
            if ((firstUnit & MAPPING_HAS_CCC_LCCC_WORD) != 0)
            {
                fcd16 |= extraData[mapping - 1] & 0xff00;  // lccc
            }
            return fcd16;
        }

        /// <summary>
        /// Gets the decomposition for one code point.
        /// </summary>
        /// <param name="c">Code point.</param>
        /// <returns><paramref name="c"/>'s decomposition, if it has one; returns null if it does not have a decomposition.</returns>
        public string GetDecomposition(int c)
        {
            int norm16;
            if (c < minDecompNoCP || IsMaybeOrNonZeroCC(norm16 = GetNorm16(c)))
            {
                // c does not decompose
                return null;
            }
            int decomp = -1;
            if (IsDecompNoAlgorithmic(norm16))
            {
                // Maps to an isCompYesAndZeroCC.
                decomp = c = MapAlgorithmic(c, norm16);
                // The mapping might decompose further.
                norm16 = GetNorm16(c);
            }
            if (norm16 < minYesNo)
            {
                if (decomp < 0)
                {
                    return null;
                }
                else
                {
                    return UTF16.ValueOf(decomp);
                }
            }
            else if (IsHangulLV(norm16) || IsHangulLVT(norm16))
            {
                // Hangul syllable: decompose algorithmically
                StringBuilder buffer = new StringBuilder();
                Hangul.Decompose(c, buffer);
                return buffer.ToString();
            }
            // c decomposes, get everything from the variable-length extra data
            int mapping = norm16 >> OFFSET_SHIFT;
            int length = extraData[mapping++] & MAPPING_LENGTH_MASK;
            return extraData.Substring(mapping, length); // ICU4N: (mapping + length) - mapping == length
        }

        /// <summary>
        /// Gets the raw decomposition for one code point.
        /// </summary>
        /// <param name="c">Code point.</param>
        /// <returns><paramref name="c"/>'s raw decomposition, if it has one; returns null if it does not have a decomposition.</returns>
        public string GetRawDecomposition(int c)
        {
            int norm16;
            if (c < minDecompNoCP || IsDecompYes(norm16 = GetNorm16(c)))
            {
                // c does not decompose
                return null;
            }
            else if (IsHangulLV(norm16) || IsHangulLVT(norm16))
            {
                // Hangul syllable: decompose algorithmically
                StringBuilder buffer = new StringBuilder();
                Hangul.GetRawDecomposition(c, buffer);
                return buffer.ToString();
            }
            else if (IsDecompNoAlgorithmic(norm16))
            {
                return UTF16.ValueOf(MapAlgorithmic(c, norm16));
            }
            // c decomposes, get everything from the variable-length extra data
            int mapping = norm16 >> OFFSET_SHIFT;
            int firstUnit = extraData[mapping];
            int mLength = firstUnit & MAPPING_LENGTH_MASK;  // length of normal mapping
            if ((firstUnit & MAPPING_HAS_RAW_MAPPING) != 0)
            {
                // Read the raw mapping from before the firstUnit and before the optional ccc/lccc word.
                // Bit 7=MAPPING_HAS_CCC_LCCC_WORD
                int rawMapping = mapping - ((firstUnit >> 7) & 1) - 1;
                char rm0 = extraData[rawMapping];
                if (rm0 <= MAPPING_LENGTH_MASK)
                {
                    return extraData.Substring(rawMapping - rm0, rm0); // ICU4N: (rawMapping - rm0) - rawMapping == rm0
                }
                else
                {
                    // Copy the normal mapping and replace its first two code units with rm0.
                    StringBuilder buffer = new StringBuilder(mLength - 1).Append(rm0);
                    mapping += 1 + 2;  // skip over the firstUnit and the first two mapping code units
                    return buffer.Append(extraData, mapping, mLength - 2).ToString(); // (mapping + mLength - 2) - mapping == mLength - 2
                }
            }
            else
            {
                mapping += 1;  // skip over the firstUnit
                return extraData.Substring(mapping, mLength); // ICU4N: (mapping + mLength) - mapping == mLength
            }
        }

        /// <summary>
        /// Returns true if code point <paramref name="c"/> starts a canonical-iterator string segment.
        /// <b><see cref="EnsureCanonIterData()"/> must have been called before this method,
        /// or else this method will crash.</b>
        /// </summary>
        /// <param name="c">A Unicode code point.</param>
        /// <returns>true if <paramref name="c"/> starts a canonical-iterator string segment.</returns>
        public bool IsCanonSegmentStarter(int c)
        {
            return canonIterData.Get(c) >= 0;
        }

        /// <summary>
        /// Returns true if there are characters whose decomposition starts with <paramref name="c"/>.
        /// If so, then the set is cleared and then filled with those characters.
        /// <b><see cref="EnsureCanonIterData()"/> must have been called before this method,
        /// or else this method will crash.</b>
        /// </summary>
        /// <param name="c">A Unicode code point.</param>
        /// <param name="set">A UnicodeSet to receive the characters whose decompositions
        /// start with <paramref name="c"/>, if there are any.</param>
        /// <returns>true if there are characters whose decomposition starts with <paramref name="c"/>.</returns>
        public bool GetCanonStartSet(int c, UnicodeSet set)
        {
            int canonValue = canonIterData.Get(c) & ~CANON_NOT_SEGMENT_STARTER;
            if (canonValue == 0)
            {
                return false;
            }
            set.Clear();
            int value = canonValue & CANON_VALUE_MASK;
            if ((canonValue & CANON_HAS_SET) != 0)
            {
                set.AddAll(canonStartSets[value]);
            }
            else if (value != 0)
            {
                set.Add(value);
            }
            if ((canonValue & CANON_HAS_COMPOSITIONS) != 0)
            {
                int norm16 = GetNorm16(c);
                if (norm16 == JAMO_L)
                {
                    int syllable = Hangul.HangulBase + (c - Hangul.JamoLBase) * Hangul.JamoVTCount;
                    set.Add(syllable, syllable + Hangul.JamoVTCount - 1);
                }
                else
                {
                    AddComposites(GetCompositionsList(norm16), set);
                }
            }
            return true;
        }

        // ICU4N TODO: API - rename constants to follow .NET Conventions ?

        // Fixed norm16 values.
        public const int MIN_YES_YES_WITH_CC = 0xfe02;
        public const int JAMO_VT = 0xfe00;
        public const int MIN_NORMAL_MAYBE_YES = 0xfc00;
        public const int JAMO_L = 2;  // offset=1 hasCompBoundaryAfter=FALSE
        public const int INERT = 1;  // offset=0 hasCompBoundaryAfter=TRUE

        // norm16 bit 0 is comp-boundary-after.
        public const int HAS_COMP_BOUNDARY_AFTER = 1;
        public const int OFFSET_SHIFT = 1;

        // For algorithmic one-way mappings, norm16 bits 2..1 indicate the
        // tccc (0, 1, >1) for quick FCC boundary-after tests.
        public const int DELTA_TCCC_0 = 0;
        public const int DELTA_TCCC_1 = 2;
        public const int DELTA_TCCC_GT_1 = 4;
        public const int DELTA_TCCC_MASK = 6;
        public const int DELTA_SHIFT = 3;

        public const int MAX_DELTA = 0x40;

        // Byte offsets from the start of the data, after the generic header.
        public const int IX_NORM_TRIE_OFFSET = 0;
        public const int IX_EXTRA_DATA_OFFSET = 1;
        public const int IX_SMALL_FCD_OFFSET = 2;
        public const int IX_RESERVED3_OFFSET = 3;
        public const int IX_TOTAL_SIZE = 7;

        // Code point thresholds for quick check codes.
        public const int IX_MIN_DECOMP_NO_CP = 8;
        public const int IX_MIN_COMP_NO_MAYBE_CP = 9;

        // Norm16 value thresholds for quick check combinations and types of extra data.

        /// <summary>Mappings &amp; compositions in [minYesNo..minYesNoMappingsOnly[.</summary>
        public const int IX_MIN_YES_NO = 10;
        /// <summary>Mappings are comp-normalized.</summary>
        public const int IX_MIN_NO_NO = 11;
        public const int IX_LIMIT_NO_NO = 12;
        public const int IX_MIN_MAYBE_YES = 13;

        /// <summary>Mappings only in [minYesNoMappingsOnly..minNoNo[.</summary>
        public const int IX_MIN_YES_NO_MAPPINGS_ONLY = 14;
        /// <summary>Mappings are not comp-normalized but have a comp boundary before.</summary>
        public const int IX_MIN_NO_NO_COMP_BOUNDARY_BEFORE = 15;
        /// <summary>Mappings do not have a comp boundary before.</summary>
        public const int IX_MIN_NO_NO_COMP_NO_MAYBE_CC = 16;
        /// <summary>Mappings to the empty string.</summary>
        public const int IX_MIN_NO_NO_EMPTY = 17;

        public const int IX_MIN_LCCC_CP = 18;
        public const int IX_COUNT = 20;

        public const int MAPPING_HAS_CCC_LCCC_WORD = 0x80;
        public const int MAPPING_HAS_RAW_MAPPING = 0x40;
        // unused bit 0x20;
        public const int MAPPING_LENGTH_MASK = 0x1f;

        public const int COMP_1_LAST_TUPLE = 0x8000;
        public const int COMP_1_TRIPLE = 1;
        public const int COMP_1_TRAIL_LIMIT = 0x3400;
        public const int COMP_1_TRAIL_MASK = 0x7ffe;
        public const int COMP_1_TRAIL_SHIFT = 9;  // 10-1 for the "triple" bit
        public const int COMP_2_TRAIL_SHIFT = 6;
        public const int COMP_2_TRAIL_MASK = 0xffc0;

        // higher-level functionality ------------------------------------------ ***

        // ICU4N specific - Decompose(ICharSequence s, StringBuilder dest) moved to Normalizer2ImplExtension.tt

        // ICU4N specific - Decompose(ICharSequence s, int src, int limit, StringBuilder dest,
        //    int destLengthEstimate) moved to Normalizer2ImplExtension.tt

        // ICU4N specific - Decompose(ICharSequence s, int src, int limit,
        //    ReorderingBuffer buffer) moved to Normalizer2ImplExtension.tt

        // ICU4N specific - DecomposeAndAppend(ICharSequence s, bool doDecompose, ReorderingBuffer buffer) moved to Normalizer2ImplExtension.tt

        // ICU4N specific - Compose(ICharSequence s, int src, int limit,
        //    bool onlyContiguous, bool doCompose, ReorderingBuffer buffer) moved to Normalizer2ImplExtension.tt

        // ICU4N specific - ComposeQuickCheck(ICharSequence s, int src, int limit,
        //    bool onlyContiguous, bool doSpan) moved to Normalizer2ImplExtension.tt

        // ICU4N specific - ComposeAndAppend(ICharSequence s, bool doCompose,
        //    bool onlyContiguous, ReorderingBuffer buffer) moved to Normalizer2ImplExtension.tt

        // ICU4N specific - MakeFCD(ICharSequence s, int src, int limit, ReorderingBuffer buffer) moved to Normalizer2ImplExtension.tt

        // ICU4N specific - MakeFCDAndAppend(ICharSequence s, bool doMakeFCD, ReorderingBuffer buffer) moved to Normalizer2ImplExtension.tt


        public bool HasDecompBoundaryBefore(int c)
        {
            return c < minLcccCP || (c <= 0xffff && !SingleLeadMightHaveNonZeroFCD16(c)) ||
                Norm16HasDecompBoundaryBefore(GetNorm16(c));
        }
        public bool Norm16HasDecompBoundaryBefore(int norm16)
        {
            if (norm16 < minNoNoCompNoMaybeCC)
            {
                return true;
            }
            if (norm16 >= limitNoNo)
            {
                return norm16 <= MIN_NORMAL_MAYBE_YES || norm16 == JAMO_VT;
            }
            // c decomposes, get everything from the variable-length extra data
            int mapping = norm16 >> OFFSET_SHIFT;
            int firstUnit = extraData[mapping];
            // true if leadCC==0 (hasFCDBoundaryBefore())
            return (firstUnit & MAPPING_HAS_CCC_LCCC_WORD) == 0 || (extraData[mapping - 1] & 0xff00) == 0;
        }
        public bool HasDecompBoundaryAfter(int c)
        {
            if (c < minDecompNoCP)
            {
                return true;
            }
            if (c <= 0xffff && !SingleLeadMightHaveNonZeroFCD16(c))
            {
                return true;
            }
            return Norm16HasDecompBoundaryAfter(GetNorm16(c));
        }
        public bool Norm16HasDecompBoundaryAfter(int norm16)
        {
            if (norm16 <= minYesNo || IsHangulLVT(norm16))
            {
                return true;
            }
            if (norm16 >= limitNoNo)
            {
                if (IsMaybeOrNonZeroCC(norm16))
                {
                    return norm16 <= MIN_NORMAL_MAYBE_YES || norm16 == JAMO_VT;
                }
                // Maps to an isCompYesAndZeroCC.
                return (norm16 & DELTA_TCCC_MASK) <= DELTA_TCCC_1;
            }
            // c decomposes, get everything from the variable-length extra data
            int mapping = norm16 >> OFFSET_SHIFT;
            int firstUnit = extraData[mapping];
            // decomp after-boundary: same as hasFCDBoundaryAfter(),
            // fcd16<=1 || trailCC==0
            if (firstUnit > 0x1ff)
            {
                return false;  // trailCC>1
            }
            if (firstUnit <= 0xff)
            {
                return true;  // trailCC==0
            }
            // if(trailCC==1) test leadCC==0, same as checking for before-boundary
            // true if leadCC==0 (hasFCDBoundaryBefore())
            return (firstUnit & MAPPING_HAS_CCC_LCCC_WORD) == 0 || (extraData[mapping - 1] & 0xff00) == 0;
        }
        public bool IsDecompInert(int c) { return IsDecompYesAndZeroCC(GetNorm16(c)); }

        public bool HasCompBoundaryBefore(int c)
        {
            return c < minCompNoMaybeCP || Norm16HasCompBoundaryBefore(GetNorm16(c));
        }
        public bool HasCompBoundaryAfter(int c, bool onlyContiguous)
        {
            return Norm16HasCompBoundaryAfter(GetNorm16(c), onlyContiguous);
        }
        public bool IsCompInert(int c, bool onlyContiguous)
        {
            int norm16 = GetNorm16(c);
            return IsCompYesAndZeroCC(norm16) &&
                (norm16 & HAS_COMP_BOUNDARY_AFTER) != 0 &&
                (!onlyContiguous || IsInert(norm16) || extraData[norm16 >> OFFSET_SHIFT] <= 0x1ff);
        }

        public bool HasFCDBoundaryBefore(int c) { return HasDecompBoundaryBefore(c); }
        public bool HasFCDBoundaryAfter(int c) { return HasDecompBoundaryAfter(c); }
        public bool IsFCDInert(int c) { return GetFCD16(c) <= 1; }

        private bool IsMaybe(int norm16) { return minMaybeYes <= norm16 && norm16 <= JAMO_VT; }
        private bool IsMaybeOrNonZeroCC(int norm16) { return norm16 >= minMaybeYes; }
        private static bool IsInert(int norm16) { return norm16 == INERT; }
        private static bool IsJamoL(int norm16) { return norm16 == JAMO_L; }
        private static bool IsJamoVT(int norm16) { return norm16 == JAMO_VT; }
        private int HangulLVT() { return minYesNoMappingsOnly | HAS_COMP_BOUNDARY_AFTER; }
        private bool IsHangulLV(int norm16) { return norm16 == minYesNo; }
        private bool IsHangulLVT(int norm16)
        {
            return norm16 == HangulLVT();
        }
        private bool IsCompYesAndZeroCC(int norm16) { return norm16 < minNoNo; }
        // UBool isCompYes(uint16_t norm16) const {
        //     return norm16>=MIN_YES_YES_WITH_CC || norm16<minNoNo;
        // }
        // UBool isCompYesOrMaybe(uint16_t norm16) const {
        //     return norm16<minNoNo || minMaybeYes<=norm16;
        // }
        // private bool hasZeroCCFromDecompYes(int norm16) {
        //     return norm16<=MIN_NORMAL_MAYBE_YES || norm16==JAMO_VT;
        // }
        private bool IsDecompYesAndZeroCC(int norm16)
        {
            return norm16 < minYesNo ||
                   norm16 == JAMO_VT ||
                   (minMaybeYes <= norm16 && norm16 <= MIN_NORMAL_MAYBE_YES);
        }
        /// <summary>
        /// A little faster and simpler than <see cref="IsDecompYesAndZeroCC(int)"/> but does not include
        /// the MaybeYes which combine-forward and have ccc=0.
        /// (Standard Unicode 10 normalization does not have such characters.)
        /// </summary>
        private bool IsMostDecompYesAndZeroCC(int norm16)
        {
            return norm16 < minYesNo || norm16 == MIN_NORMAL_MAYBE_YES || norm16 == JAMO_VT;
        }
        private bool IsDecompNoAlgorithmic(int norm16) { return norm16 >= limitNoNo; }

        // For use with isCompYes().
        // Perhaps the compiler can combine the two tests for MIN_YES_YES_WITH_CC.
        // static uint8_t getCCFromYes(uint16_t norm16) {
        //     return norm16>=MIN_YES_YES_WITH_CC ? getCCFromNormalYesOrMaybe(norm16) : 0;
        // }
        private int GetCCFromNoNo(int norm16)
        {
            int mapping = norm16 >> OFFSET_SHIFT;
            if ((extraData[mapping] & MAPPING_HAS_CCC_LCCC_WORD) != 0)
            {
                return extraData[mapping - 1] & 0xff;
            }
            else
            {
                return 0;
            }
        }
        int GetTrailCCFromCompYesAndZeroCC(int norm16)
        {
            if (norm16 <= minYesNo)
            {
                return 0;  // yesYes and Hangul LV have ccc=tccc=0
            }
            else
            {
                // For Hangul LVT we harmlessly fetch a firstUnit with tccc=0 here.
                return extraData[norm16 >> OFFSET_SHIFT] >> 8;  // tccc from yesNo
            }
        }

        // Requires algorithmic-NoNo.
        private int MapAlgorithmic(int c, int norm16)
        {
            return c + (norm16 >> DELTA_SHIFT) - centerNoNoDelta;
        }

        // Requires minYesNo<norm16<limitNoNo.
        // private int getMapping(int norm16) { return extraData+(norm16>>OFFSET_SHIFT); }

        /// <returns>Index into maybeYesCompositions, or -1.</returns>
        private int GetCompositionsListForDecompYes(int norm16)
        {
            if (norm16 < JAMO_L || MIN_NORMAL_MAYBE_YES <= norm16)
            {
                return -1;
            }
            else
            {
                if ((norm16 -= minMaybeYes) < 0)
                {
                    // norm16<minMaybeYes: index into extraData which is a substring at
                    //     maybeYesCompositions[MIN_NORMAL_MAYBE_YES-minMaybeYes]
                    // same as (MIN_NORMAL_MAYBE_YES-minMaybeYes)+norm16
                    norm16 += MIN_NORMAL_MAYBE_YES;  // for yesYes; if Jamo L: harmless empty list
                }
                return norm16 >> OFFSET_SHIFT;
            }
        }
        /// <returns>Index into maybeYesCompositions.</returns>
        private int GetCompositionsListForComposite(int norm16)
        {
            // A composite has both mapping & compositions list.
            int list = ((MIN_NORMAL_MAYBE_YES - minMaybeYes) + norm16) >> OFFSET_SHIFT;
            int firstUnit = maybeYesCompositions[list];
            return list +  // mapping in maybeYesCompositions
                1 +  // +1 to skip the first unit with the mapping length
                (firstUnit & MAPPING_LENGTH_MASK);  // + mapping length
        }
        private int GetCompositionsListForMaybe(int norm16)
        {
            // minMaybeYes<=norm16<MIN_NORMAL_MAYBE_YES
            return (norm16 - minMaybeYes) >> OFFSET_SHIFT;
        }

        /// <param name="norm16">Code point must have compositions.</param>
        /// <returns>Index into maybeYesCompositions.</returns>
        private int GetCompositionsList(int norm16)
        {
            return IsDecompYes(norm16) ?
                    GetCompositionsListForDecompYes(norm16) :
                    GetCompositionsListForComposite(norm16);
        }

        // ICU4N specific - DecomposeShort(ICharSequence s, int src, int limit,
        //    bool stopAtCompBoundary, bool onlyContiguous, ReorderingBuffer buffer) moved to Normalizer2ImplExtention.tt


        private void Decompose(int c, int norm16, ReorderingBuffer buffer)
        {
            // get the decomposition and the lead and trail cc's
            if (norm16 >= limitNoNo)
            {
                if (IsMaybeOrNonZeroCC(norm16))
                {
                    buffer.Append(c, GetCCFromYesOrMaybe(norm16));
                    return;
                }
                // Maps to an isCompYesAndZeroCC.
                c = MapAlgorithmic(c, norm16);
                norm16 = GetNorm16(c);
            }
            if (norm16 < minYesNo)
            {
                // c does not decompose
                buffer.Append(c, 0);
            }
            else if (IsHangulLV(norm16) || IsHangulLVT(norm16))
            {
                // Hangul syllable: decompose algorithmically
                Hangul.Decompose(c, buffer);
            }
            else
            {
                // c decomposes, get everything from the variable-length extra data
                int mapping = norm16 >> OFFSET_SHIFT;
                int firstUnit = extraData[mapping];
                int length = firstUnit & MAPPING_LENGTH_MASK;
                int leadCC, trailCC;
                trailCC = firstUnit >> 8;
                if ((firstUnit & MAPPING_HAS_CCC_LCCC_WORD) != 0)
                {
                    leadCC = extraData[mapping - 1] >> 8;
                }
                else
                {
                    leadCC = 0;
                }
                ++mapping;  // skip over the firstUnit
                buffer.Append(extraData, mapping, mapping + length, leadCC, trailCC);
            }
        }

        /// <summary>
        /// Finds the recomposition result for
        /// a forward-combining "lead" character,
        /// specified with a pointer to its compositions list,
        /// and a backward-combining "trail" character.
        /// </summary>
        /// <remarks>
        /// If the lead and trail characters combine, then this function returns
        /// the following "compositeAndFwd" value:
        /// <code>
        /// Bits 21..1  composite character
        /// Bit      0  set if the composite is a forward-combining starter
        /// </code>
        /// otherwise it returns -1.
        /// <para/>
        /// The compositions list has (trail, compositeAndFwd) pair entries,
        /// encoded as either pairs or triples of 16-bit units.
        /// The last entry has the high bit of its first unit set.
        /// <para/>
        /// The list is sorted by ascending trail characters (there are no duplicates).
        /// A linear search is used.
        /// <para/>
        /// See normalizer2impl.h for a more detailed description
        /// of the compositions list format.
        /// </remarks>
        private static int Combine(string compositions, int list, int trail)
        {
            int key1, firstUnit;
            if (trail < COMP_1_TRAIL_LIMIT)
            {
                // trail character is 0..33FF
                // result entry may have 2 or 3 units
                key1 = (trail << 1);
                while (key1 > (firstUnit = compositions[list]))
                {
                    list += 2 + (firstUnit & COMP_1_TRIPLE);
                }
                if (key1 == (firstUnit & COMP_1_TRAIL_MASK))
                {
                    if ((firstUnit & COMP_1_TRIPLE) != 0)
                    {
                        return (compositions[list + 1] << 16) | compositions[list + 2];
                    }
                    else
                    {
                        return compositions[list + 1];
                    }
                }
            }
            else
            {
                // trail character is 3400..10FFFF
                // result entry has 3 units
                key1 = COMP_1_TRAIL_LIMIT + (((trail >> COMP_1_TRAIL_SHIFT)) & ~COMP_1_TRIPLE);
                int key2 = (trail << COMP_2_TRAIL_SHIFT) & 0xffff;
                int secondUnit;
                for (; ; )
                {
                    if (key1 > (firstUnit = compositions[list]))
                    {
                        list += 2 + (firstUnit & COMP_1_TRIPLE);
                    }
                    else if (key1 == (firstUnit & COMP_1_TRAIL_MASK))
                    {
                        if (key2 > (secondUnit = compositions[list + 1]))
                        {
                            if ((firstUnit & COMP_1_LAST_TUPLE) != 0)
                            {
                                break;
                            }
                            else
                            {
                                list += 3;
                            }
                        }
                        else if (key2 == (secondUnit & COMP_2_TRAIL_MASK))
                        {
                            return ((secondUnit & ~COMP_2_TRAIL_MASK) << 16) | compositions[list + 2];
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return -1;
        }
        /// <param name="list">Some character's compositions list.</param>
        /// <param name="set">Recursively receives the composites from these compositions.</param>
        private void AddComposites(int list, UnicodeSet set)
        {
            int firstUnit, compositeAndFwd;
            do
            {
                firstUnit = maybeYesCompositions[list];
                if ((firstUnit & COMP_1_TRIPLE) == 0)
                {
                    compositeAndFwd = maybeYesCompositions[list + 1];
                    list += 2;
                }
                else
                {
                    compositeAndFwd = ((maybeYesCompositions[list + 1] & ~COMP_2_TRAIL_MASK) << 16) |
                                    maybeYesCompositions[list + 2];
                    list += 3;
                }
                int composite = compositeAndFwd >> 1;
                if ((compositeAndFwd & 1) != 0)
                {
                    AddComposites(GetCompositionsListForComposite(GetNorm16(composite)), set);
                }
                set.Add(composite);
            } while ((firstUnit & COMP_1_LAST_TUPLE) == 0);
        }
        /// <summary>
        /// Recomposes the buffer text starting at <paramref name="recomposeStartIndex"/>
        /// (which is in NFD - decomposed and canonically ordered),
        /// and truncates the buffer contents.
        /// </summary>
        /// <remarks>
        /// Note that recomposition never lengthens the text:
        /// Any character consists of either one or two code units;
        /// a composition may contain at most one more code unit than the original starter,
        /// while the combining mark that is removed has at least one code unit.
        /// </remarks>
        private void Recompose(ReorderingBuffer buffer, int recomposeStartIndex,
                               bool onlyContiguous)
        {
            StringBuilder sb = buffer.StringBuilder;
            int p = recomposeStartIndex;
            if (p == sb.Length)
            {
                return;
            }

            int starter, pRemove;
            int compositionsList;
            int c, compositeAndFwd;
            int norm16;
            int cc, prevCC;
            bool starterIsSupplementary;

            // Some of the following variables are not used until we have a forward-combining starter
            // and are only initialized now to avoid compiler warnings.
            compositionsList = -1;  // used as indicator for whether we have a forward-combining starter
            starter = -1;
            starterIsSupplementary = false;
            prevCC = 0;

            for (; ; )
            {
                c = sb.CodePointAt(p);
                p += Character.CharCount(c);
                norm16 = GetNorm16(c);
                cc = GetCCFromYesOrMaybe(norm16);
                if ( // this character combines backward and
                    IsMaybe(norm16) &&
                    // we have seen a starter that combines forward and
                    compositionsList >= 0 &&
                    // the backward-combining character is not blocked
                    (prevCC < cc || prevCC == 0)
                )
                {
                    if (IsJamoVT(norm16))
                    {
                        // c is a Jamo V/T, see if we can compose it with the previous character.
                        if (c < Hangul.JamoTBase)
                        {
                            // c is a Jamo Vowel, compose with previous Jamo L and following Jamo T.
                            char prev = (char)(sb[starter] - Hangul.JamoLBase);
                            if (prev < Hangul.JamoLCount)
                            {
                                pRemove = p - 1;
                                char syllable = (char)
                                    (Hangul.HangulBase +
                                     (prev * Hangul.JamoVCount + (c - Hangul.JamoVBase)) *
                                     Hangul.JamoTCount);
                                char t;
                                if (p != sb.Length && (t = (char)(sb[p] - Hangul.JamoTBase)) < Hangul.JamoTCount)
                                {
                                    ++p;
                                    syllable += t;  // The next character was a Jamo T.
                                }
                                //sb.setCharAt(starter, syllable);
                                sb[starter] = syllable;
                                // remove the Jamo V/T
                                sb.Delete(pRemove, p - pRemove); // ICU4N: Corrected 2nd parameter
                                p = pRemove;
                            }
                        }
                        /*
                         * No "else" for Jamo T:
                         * Since the input is in NFD, there are no Hangul LV syllables that
                         * a Jamo T could combine with.
                         * All Jamo Ts are combined above when handling Jamo Vs.
                         */
                        if (p == sb.Length)
                        {
                            break;
                        }
                        compositionsList = -1;
                        continue;
                    }
                    else if ((compositeAndFwd = Combine(maybeYesCompositions, compositionsList, c)) >= 0)
                    {
                        // The starter and the combining mark (c) do combine.
                        int composite = compositeAndFwd >> 1;

                        // Remove the combining mark.
                        pRemove = p - Character.CharCount(c);  // pRemove & p: start & limit of the combining mark
                        sb.Delete(pRemove, p - pRemove); // ICU4N: Corrected 2nd parameter
                        p = pRemove;
                        // Replace the starter with the composite.
                        if (starterIsSupplementary)
                        {
                            if (composite > 0xffff)
                            {
                                // both are supplementary
                                sb[starter] = UTF16.GetLeadSurrogate(composite);
                                sb[starter + 1] = UTF16.GetTrailSurrogate(composite);
                            }
                            else
                            {
                                sb[starter] = (char)c;

                                //sb.deleteCharAt(starter + 1);
                                sb.Remove(starter + 1, 1);
                                // The composite is shorter than the starter,
                                // move the intermediate characters forward one.
                                starterIsSupplementary = false;
                                --p;
                            }
                        }
                        else if (composite > 0xffff)
                        {
                            // The composite is longer than the starter,
                            // move the intermediate characters back one.
                            starterIsSupplementary = true;
                            sb[starter] = UTF16.GetLeadSurrogate(composite);
                            sb.Insert(starter + 1, UTF16.GetTrailSurrogate(composite));
                            ++p;
                        }
                        else
                        {
                            // both are on the BMP
                            sb[starter] = (char)composite;
                        }

                        // Keep prevCC because we removed the combining mark.

                        if (p == sb.Length)
                        {
                            break;
                        }
                        // Is the composite a starter that combines forward?
                        if ((compositeAndFwd & 1) != 0)
                        {
                            compositionsList =
                                GetCompositionsListForComposite(GetNorm16(composite));
                        }
                        else
                        {
                            compositionsList = -1;
                        }

                        // We combined; continue with looking for compositions.
                        continue;
                    }
                }

                // no combination this time
                prevCC = cc;
                if (p == sb.Length)
                {
                    break;
                }

                // If c did not combine, then check if it is a starter.
                if (cc == 0)
                {
                    // Found a new starter.
                    if ((compositionsList = GetCompositionsListForDecompYes(norm16)) >= 0)
                    {
                        // It may combine with something, prepare for it.
                        if (c <= 0xffff)
                        {
                            starterIsSupplementary = false;
                            starter = p - 1;
                        }
                        else
                        {
                            starterIsSupplementary = true;
                            starter = p - 2;
                        }
                    }
                }
                else if (onlyContiguous)
                {
                    // FCC: no discontiguous compositions; any intervening character blocks.
                    compositionsList = -1;
                }
            }
            buffer.Flush();
        }

        public int ComposePair(int a, int b)
        {
            int norm16 = GetNorm16(a);  // maps an out-of-range 'a' to inert norm16=0
            int list;
            if (IsInert(norm16))
            {
                return -1;
            }
            else if (norm16 < minYesNoMappingsOnly)
            {
                // a combines forward.
                if (IsJamoL(norm16))
                {
                    b -= Hangul.JamoVBase;
                    if (0 <= b && b < Hangul.JamoVCount)
                    {
                        return
                            (Hangul.HangulBase +
                             ((a - Hangul.JamoLBase) * Hangul.JamoVCount + b) *
                             Hangul.JamoTCount);
                    }
                    else
                    {
                        return -1;
                    }
                }
                else if (IsHangulLV(norm16))
                {
                    b -= Hangul.JamoTBase;
                    if (0 < b && b < Hangul.JamoTCount)
                    {  // not b==0!
                        return a + b;
                    }
                    else
                    {
                        return -1;
                    }
                }
                else
                {
                    // 'a' has a compositions list in extraData
                    list = ((MIN_NORMAL_MAYBE_YES - minMaybeYes) + norm16) >> OFFSET_SHIFT;
                    if (norm16 > minYesNo)
                    {  // composite 'a' has both mapping & compositions list
                        list +=  // mapping pointer
                            1 +  // +1 to skip the first unit with the mapping length
                            (maybeYesCompositions[list] & MAPPING_LENGTH_MASK);  // + mapping length
                    }
                }
            }
            else if (norm16 < minMaybeYes || MIN_NORMAL_MAYBE_YES <= norm16)
            {
                return -1;
            }
            else
            {
                list = GetCompositionsListForMaybe(norm16);  // offset into maybeYesCompositions
            }
            if (b < 0 || 0x10ffff < b)
            {  // combine(list, b) requires a valid code point b
                return -1;
            }
            return Combine(maybeYesCompositions, list, b) >> 1;
        }
        /// <summary>
        /// Does <paramref name="c"/> have a composition boundary before it?
        /// True if its decomposition begins with a character that has
        /// ccc=0 &amp;&amp; NFC_QC=Yes (<see cref="IsCompYesAndZeroCC(int)"/>).
        /// As a shortcut, this is true if <paramref name="c"/> itself has ccc=0 &amp;&amp; NFC_QC=Yes
        /// (<see cref="IsCompYesAndZeroCC(int)"/>) so we need not decompose.
        /// </summary>
        private bool HasCompBoundaryBefore(int c, int norm16)
        {
            return c < minCompNoMaybeCP || Norm16HasCompBoundaryBefore(norm16);
        }
        private bool Norm16HasCompBoundaryBefore(int norm16)
        {
            return norm16 < minNoNoCompNoMaybeCC || IsAlgorithmicNoNo(norm16);
        }

        // ICU4N specific - HasCompBoundaryBefore(ICharSequence s, int src, int limit) moved to Normalizer2ImplExtention.tt

        private bool Norm16HasCompBoundaryAfter(int norm16, bool onlyContiguous)
        {
            return (norm16 & HAS_COMP_BOUNDARY_AFTER) != 0 &&
                (!onlyContiguous || IsTrailCC01ForCompBoundaryAfter(norm16));
        }

        // ICU4N specific - HasCompBoundaryAfter(ICharSequence s, int start, int p, bool onlyContiguous) moved to Normalizer2ImplExtention.tt

        /// <summary>For FCC: Given norm16 HAS_COMP_BOUNDARY_AFTER, does it have tccc&lt;=1?</summary>
        private bool IsTrailCC01ForCompBoundaryAfter(int norm16)
        {
            return IsInert(norm16) || (IsDecompNoAlgorithmic(norm16) ?
                (norm16 & DELTA_TCCC_MASK) <= DELTA_TCCC_1 : extraData[norm16 >> OFFSET_SHIFT] <= 0x1ff);
        }

        // ICU4N specific - FindPreviousCompBoundary(ICharSequence s, int p, bool onlyContiguous) moved to Normalizer2ImplExtention.tt

        // ICU4N specific - FindNextCompBoundary(ICharSequence s, int p, int limit, bool onlyContiguous) moved to Normalizer2ImplExtention.tt

        // ICU4N specific - FindPreviousFCDBoundary(ICharSequence s, int p) moved to Normalizer2ImplExtention.tt

        // ICU4N specific - FindNextFCDBoundary(ICharSequence s, int p, int limit) moved to Normalizer2ImplExtention.tt

        // ICU4N specific - GetPreviousTrailCC(ICharSequence s, int start, int p) moved to Normalizer2ImplExtention.tt


        private void AddToStartSet(Trie2Writable newData, int origin, int decompLead)
        {
            int canonValue = newData.Get(decompLead);
            if ((canonValue & (CANON_HAS_SET | CANON_VALUE_MASK)) == 0 && origin != 0)
            {
                // origin is the first character whose decomposition starts with
                // the character for which we are setting the value.
                newData.Set(decompLead, canonValue | origin);
            }
            else
            {
                // origin is not the first character, or it is U+0000.
                UnicodeSet set;
                if ((canonValue & CANON_HAS_SET) == 0)
                {
                    int firstOrigin = canonValue & CANON_VALUE_MASK;
                    canonValue = (canonValue & ~CANON_VALUE_MASK) | CANON_HAS_SET | canonStartSets.Count;
                    newData.Set(decompLead, canonValue);
                    canonStartSets.Add(set = new UnicodeSet());
                    if (firstOrigin != 0)
                    {
                        set.Add(firstOrigin);
                    }
                }
                else
                {
                    set = canonStartSets[canonValue & CANON_VALUE_MASK];
                }
                set.Add(origin);
            }
        }

        private VersionInfo dataVersion;

        // BMP code point thresholds for quick check loops looking at single UTF-16 code units.
        private int minDecompNoCP;
        private int minCompNoMaybeCP;
        private int minLcccCP;

        // Norm16 value thresholds for quick check combinations and types of extra data.
        private int minYesNo;
        private int minYesNoMappingsOnly;
        private int minNoNo;
        private int minNoNoCompBoundaryBefore;
        private int minNoNoCompNoMaybeCC;
        private int minNoNoEmpty;
        private int limitNoNo;
        private int centerNoNoDelta;
        private int minMaybeYes;

        private Trie2_16 normTrie;
        private string maybeYesCompositions;
        private string extraData;  // mappings and/or compositions for yesYes, yesNo & noNo characters
        private byte[] smallFCD;  // [0x100] one bit per 32 BMP code points, set if any FCD!=0

        private Trie2_32 canonIterData;
        private IList<UnicodeSet> canonStartSets;

        // bits in canonIterData
        private const int CANON_NOT_SEGMENT_STARTER = unchecked((int)0x80000000);
        private const int CANON_HAS_COMPOSITIONS = 0x40000000;
        private const int CANON_HAS_SET = 0x200000;
        private const int CANON_VALUE_MASK = 0x1fffff;
    }
}
