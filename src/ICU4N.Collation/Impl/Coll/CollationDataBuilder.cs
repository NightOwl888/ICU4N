using ICU4N.Support.Text;
using ICU4N.Text;
using ICU4N.Util;
using J2N;
using J2N.Collections;
using J2N.Numerics;
using J2N.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ICU4N.Impl.Coll
{
    /// <summary>
    /// Low-level <see cref="CollationData"/> builder.
    /// Takes (character, CE) pairs and builds them into runtime data structures.
    /// Supports characters with context prefixes and contraction suffixes.
    /// </summary>
    internal sealed class CollationDataBuilder // not final in C++
    {
        /// <summary>
        /// Collation element modifier. Interface class for a modifier
        /// that changes a tailoring builder's temporary CEs to final CEs.
        /// Called for every non-special CE32 and every expansion CE.
        /// </summary>
        internal interface ICEModifier
        {
            /// <summary>
            /// Returns a new CE to replace the non-special input CE32, or else <see cref="Collation.NoCE"/>.
            /// </summary>
            /// <param name="ce32"></param>
            /// <returns></returns>
            long ModifyCE32(int ce32);
            /// <summary>
            /// Returns a new CE to replace the input CE, or else <see cref="Collation.NoCE"/>.
            /// </summary>
            long ModifyCE(long ce);
        }

        internal CollationDataBuilder()
        {
            nfcImpl = Norm2AllModes.GetNFCInstance().Impl;
            base_ = null;
            //baseSettings = null; // ICU4N specific - not used
            trie = null;
            ce32s = new List<int>(32);
            ce64s = new List<long>(32);
            conditionalCE32s = new List<ConditionalCE32>();
            modified = false;
            fastLatinEnabled = false;
            fastLatinBuilder = null;
            collIter = null;
            // Reserve the first CE32 for U+0000.
            ce32s.Add(0);
        }

        internal void InitForTailoring(CollationData b)
        {
            if (trie != null)
            {
                throw new InvalidOperationException("attempt to reuse a CollationDataBuilder");
            }
            if (b == null)
            {
                throw new ArgumentException("null CollationData");
            }
            base_ = b;

            // For a tailoring, the default is to fall back to the base.
            trie = new Trie2Writable(Collation.FALLBACK_CE32, Collation.FFFD_CE32);

            // Set the Latin-1 letters block so that it is allocated first in the data array,
            // to try to improve locality of reference when sorting Latin-1 text.
            // Do not use utrie2_setRange32() since that will not actually allocate blocks
            // that are filled with the default value.
            // ASCII (0..7F) is already preallocated anyway.
            for (int c = 0xc0; c <= 0xff; ++c)
            {
                trie.Set(c, Collation.FALLBACK_CE32);
            }

            // Hangul syllables are not tailorable (except via tailoring Jamos).
            // Always set the Hangul tag to help performance.
            // Do this here, rather than in buildMappings(),
            // so that we see the HANGUL_TAG in various assertions.
            int hangulCE32 = Collation.MakeCE32FromTagAndIndex(Collation.HANGUL_TAG, 0);
            trie.SetRange(Hangul.HangulBase, Hangul.HangulEnd, hangulCE32, true);

            // Copy the set contents but don't copy/clone the set as a whole because
            // that would copy the isFrozen state too.
            unsafeBackwardSet.AddAll(b.unsafeBackwardSet);
        }

        internal bool IsCompressibleLeadByte(int b)
        {
            return base_.IsCompressibleLeadByte(b);
        }

        internal bool IsCompressiblePrimary(long p)
        {
            return IsCompressibleLeadByte(((int)p).TripleShift(24));
        }

        /// <summary>
        /// <c>true</c> if this builder has mappings (e.g., <see cref="Add(ICharSequence, ICharSequence, long[], int)"/> has been called)
        /// </summary>
        internal bool HasMappings => modified;

        /// <returns><c>true</c> if c has CEs in this builder.</returns>
        internal bool IsAssigned(int c)
        {
            return Collation.IsAssignedCE32(trie.Get(c));
        }

        internal void Add(ICharSequence prefix, ICharSequence s, long[] ces, int cesLength)
        {
            int ce32 = EncodeCEs(ces, cesLength);
            AddCE32(prefix, s, ce32);
        }

        /// <summary>
        /// Encodes the ces as either the returned ce32 by itself,
        /// or by storing an expansion, with the returned ce32 referring to that.
        /// <para/>
        /// <c>Add(p, s, ces, cesLength) = AddCE32(p, s, EncodeCEs(ces, cesLength))</c>
        /// </summary>
        /// <param name="ces"></param>
        /// <param name="cesLength"></param>
        /// <returns></returns>
        internal int EncodeCEs(long[] ces, int cesLength)
        {
            if (cesLength < 0 || cesLength > Collation.MAX_EXPANSION_LENGTH)
            {
                throw new ArgumentException("mapping to too many CEs");
            }
            if (!IsMutable)
            {
                throw new InvalidOperationException("attempt to add mappings after Build()");
            }
            if (cesLength == 0)
            {
                // Convenience: We cannot map to nothing, but we can map to a completely ignorable CE.
                // Do this here so that callers need not do it.
                return EncodeOneCEAsCE32(0);
            }
            else if (cesLength == 1)
            {
                return EncodeOneCE(ces[0]);
            }
            else if (cesLength == 2)
            {
                // Try to encode two CEs as one CE32.
                long ce0 = ces[0];
                long ce1 = ces[1];
                long p0 = ce0.TripleShift(32);
                if ((ce0 & 0xffffffffff00ffL) == Collation.COMMON_SECONDARY_CE &&
                        (ce1 & unchecked((long)0xffffffff00ffffffL)) == Collation.COMMON_TERTIARY_CE &&
                        p0 != 0)
                {
                    // Latin mini expansion
                    return
                        (int)p0 |
                        (((int)ce0 & 0xff00) << 8) |
                        (((int)ce1 >> 16) & 0xff00) |
                        Collation.SPECIAL_CE32_LOW_BYTE |
                        Collation.LATIN_EXPANSION_TAG;
                }
            }
            // Try to encode two or more CEs as CE32s.
            int[] newCE32s = new int[Collation.MAX_EXPANSION_LENGTH];  // TODO: instance field?
            for (int i = 0; ; ++i)
            {
                if (i == cesLength)
                {
                    return EncodeExpansion32(newCE32s, 0, cesLength);
                }
                int ce32 = EncodeOneCEAsCE32(ces[i]);
                if (ce32 == Collation.NO_CE32) { break; }
                newCE32s[i] = ce32;
            }
            return EncodeExpansion(ces, 0, cesLength);
        }

        internal void AddCE32(ICharSequence prefix, ICharSequence s, int ce32)
        {
            if (s.Length == 0)
            {
                throw new ArgumentException("mapping from empty string");
            }
            if (!IsMutable)
            {
                throw new InvalidOperationException("attempt to add mappings after Build()");
            }
            int c = Character.CodePointAt(s, 0);
            int cLength = Character.CharCount(c);
            int oldCE32 = trie.Get(c);
            bool hasContext = prefix.Length != 0 || s.Length > cLength;
            if (oldCE32 == Collation.FALLBACK_CE32)
            {
                // First tailoring for c.
                // If c has contextual base mappings or if we add a contextual mapping,
                // then copy the base mappings.
                // Otherwise we just override the base mapping.
                int baseCE32 = base_.GetFinalCE32(base_.GetCE32(c));
                if (hasContext || Collation.Ce32HasContext(baseCE32))
                {
                    oldCE32 = CopyFromBaseCE32(c, baseCE32, true);
                    trie.Set(c, oldCE32);
                }
            }
            if (!hasContext)
            {
                // No prefix, no contraction.
                if (!IsBuilderContextCE32(oldCE32))
                {
                    trie.Set(c, ce32);
                }
                else
                {
                    ConditionalCE32 cond = GetConditionalCE32ForCE32(oldCE32);
                    cond.BuiltCE32 = Collation.NO_CE32;
                    cond.Ce32 = ce32;
                }
            }
            else
            {
                ConditionalCE32 cond;
                if (!IsBuilderContextCE32(oldCE32))
                {
                    // Replace the simple oldCE32 with a builder context CE32
                    // pointing to a new ConditionalCE32 list head.
                    int index = AddConditionalCE32("\0", oldCE32);
                    int contextCE32 = MakeBuilderContextCE32(index);
                    trie.Set(c, contextCE32);
                    contextChars.Add(c);
                    cond = GetConditionalCE32(index);
                }
                else
                {
                    cond = GetConditionalCE32ForCE32(oldCE32);
                    cond.BuiltCE32 = Collation.NO_CE32;
                }
                ICharSequence suffix = s.Subsequence(cLength, s.Length - cLength); // ICU4N: Corrected 2nd parameter
                string context = new StringBuilder().Append((char)prefix.Length).
                        Append(prefix).Append(suffix).ToString();
                unsafeBackwardSet.AddAll(suffix);
                for (; ; )
                {
                    // invariant: context > cond.context
                    int next = cond.Next;
                    if (next < 0)
                    {
                        // Append a new ConditionalCE32 after cond.
                        int index = AddConditionalCE32(context, ce32);
                        cond.Next = index;
                        break;
                    }
                    ConditionalCE32 nextCond = GetConditionalCE32(next);
                    int cmp = context.CompareToOrdinal(nextCond.Context);
                    if (cmp < 0)
                    {
                        // Insert a new ConditionalCE32 between cond and nextCond.
                        int index = AddConditionalCE32(context, ce32);
                        cond.Next = index;
                        GetConditionalCE32(index).Next = next;
                        break;
                    }
                    else if (cmp == 0)
                    {
                        // Same context as before, overwrite its ce32.
                        nextCond.Ce32 = ce32;
                        break;
                    }
                    cond = nextCond;
                }
            }
            modified = true;
        }

        /// <summary>
        /// Copies all mappings from the src builder, with modifications.
        /// This builder here must not be built yet, and should be empty.
        /// </summary>
        internal void CopyFrom(CollationDataBuilder src, ICEModifier modifier)
        {
            if (!IsMutable)
            {
                throw new InvalidOperationException("attempt to CopyFrom() after Build()");
            }
            CopyHelper helper = new CopyHelper(src, this, modifier);
            using (IEnumerator<Trie2Range> trieIterator = src.trie.GetEnumerator())
            {
                Trie2Range range;
                while (trieIterator.MoveNext() && !(range = trieIterator.Current).IsLeadSurrogate)
                {
                    EnumRangeForCopy(range.StartCodePoint, range.EndCodePoint, range.Value, helper);
                }
            }
            // Update the contextChars and the unsafeBackwardSet while copying,
            // in case a character had conditional mappings in the source builder
            // and they were removed later.
            modified |= src.modified;
        }

        internal void Optimize(UnicodeSet set)
        {
            if (set.IsEmpty) { return; }
            UnicodeSetIterator iter = new UnicodeSetIterator(set);
            while (iter.Next() && iter.Codepoint != UnicodeSetIterator.IsString)
            {
                int c = iter.Codepoint;
                int ce32 = trie.Get(c);
                if (ce32 == Collation.FALLBACK_CE32)
                {
                    ce32 = base_.GetFinalCE32(base_.GetCE32(c));
                    ce32 = CopyFromBaseCE32(c, ce32, true);
                    trie.Set(c, ce32);
                }
            }
            modified = true;
        }

        internal void SuppressContractions(UnicodeSet set)
        {
            if (set.IsEmpty) { return; }
            UnicodeSetIterator iter = new UnicodeSetIterator(set);
            while (iter.Next() && iter.Codepoint != UnicodeSetIterator.IsString)
            {
                int c = iter.Codepoint;
                int ce32 = trie.Get(c);
                if (ce32 == Collation.FALLBACK_CE32)
                {
                    ce32 = base_.GetFinalCE32(base_.GetCE32(c));
                    if (Collation.Ce32HasContext(ce32))
                    {
                        ce32 = CopyFromBaseCE32(c, ce32, false /* without context */);
                        trie.Set(c, ce32);
                    }
                }
                else if (IsBuilderContextCE32(ce32))
                {
                    ce32 = GetConditionalCE32ForCE32(ce32).Ce32;
                    // Simply abandon the list of ConditionalCE32.
                    // The caller will copy this builder in the end,
                    // eliminating unreachable data.
                    trie.Set(c, ce32);
                    contextChars.Remove(c);
                }
            }
            modified = true;
        }

        internal void EnableFastLatin() { fastLatinEnabled = true; }
        internal void Build(CollationData data)
        {
            BuildMappings(data);
            if (base_ != null)
            {
                data.numericPrimary = base_.numericPrimary;
                data.CompressibleBytes = base_.CompressibleBytes;
                data.numScripts = base_.numScripts;
                data.scriptsIndex = base_.scriptsIndex;
                data.scriptStarts = base_.scriptStarts;
            }
            BuildFastLatinTable(data);
        }

        /// <summary>
        /// Looks up CEs for s and appends them to the ces array.
        /// Does not handle normalization: s should be in FCD form.
        /// <para/>
        /// Does not write completely ignorable CEs.
        /// Does not write beyond <see cref="Collation.MAX_EXPANSION_LENGTH"/>.
        /// </summary>
        /// <returns>Incremented cesLength.</returns>
        internal int GetCEs(ICharSequence s, long[] ces, int cesLength)
        {
            return GetCEs(s, 0, ces, cesLength);
        }

        internal int GetCEs(ICharSequence prefix, ICharSequence s, long[] ces, int cesLength)
        {
            int prefixLength = prefix.Length;
            if (prefixLength == 0)
            {
                return GetCEs(s, 0, ces, cesLength);
            }
            else
            {
                return GetCEs(new StringBuilder(prefix.Length).Append(prefix).Append(s).AsCharSequence(), prefixLength, ces, cesLength);
            }
        }

        /// <summary>
        /// Build-time context and CE32 for a code point.
        /// If a code point has contextual mappings, then the default (no-context) mapping
        /// and all conditional mappings are stored in a singly-linked list
        /// of ConditionalCE32, sorted by context strings.
        /// <para/>
        /// Context strings sort by prefix length, then by prefix, then by contraction suffix.
        /// Context strings must be unique and in ascending order.
        /// </summary>
        private sealed class ConditionalCE32
        {
            internal ConditionalCE32(string ct, int ce)
            {
                Context = ct;
                Ce32 = ce;
                DefaultCE32 = Collation.NO_CE32;
                BuiltCE32 = Collation.NO_CE32;
                Next = -1;
            }

            internal bool HasContext => Context.Length > 1;
            internal int PrefixLength => Context[0];

            /// <summary>
            /// "\0" for the first entry for any code point, with its default CE32.
            /// <para/>
            /// Otherwise one unit with the length of the prefix string,
            /// then the prefix string, then the contraction suffix.
            /// </summary>
            internal string Context { get; set; }

            /// <summary>
            /// CE32 for the code point and its context.
            /// Can be special (e.g., for an expansion) but not contextual (prefix or contraction tag).
            /// </summary>
            internal int Ce32 { get; set; }

            /// <summary>
            /// Default CE32 for all contexts with this same prefix.
            /// Initially <see cref="Collation.NO_CE32"/>. Set only while building runtime data structures,
            /// and only on one of the nodes of a sub-list with the same prefix.
            /// </summary>
            internal int DefaultCE32 { get; set; }

            /// <summary>
            /// CE32 for the built contexts.
            /// When fetching CEs from the builder, the contexts are built into their runtime form
            /// so that the normal collation implementation can process them.
            /// The result is cached in the list head. It is reset when the contexts are modified.
            /// </summary>
            internal int BuiltCE32 { get; set; }

            /// <summary>
            /// Index of the next <see cref="ConditionalCE32"/>.
            /// Negative for the end of the list.
            /// </summary>
            internal int Next { get; set; }
        }

        internal int GetCE32FromOffsetCE32(bool fromBase, int c, int ce32) // ICU4N: In Java, this was protected, but this is a sealed class
        {
            int i = Collation.IndexFromCE32(ce32);
            long dataCE = fromBase ? base_.ces[i] : ce64s[i];
            long p = Collation.GetThreeBytePrimaryForOffsetData(c, dataCE);
            return Collation.MakeLongPrimaryCE32(p);
        }

        internal int AddCE(long ce) // ICU4N: In Java, this was protected, but this is a sealed class
        {
            int length = ce64s.Count;
            for (int i = 0; i < length; ++i)
            {
                if (ce == ce64s[i]) { return i; }
            }
            ce64s.Add(ce);
            return length;
        }

        internal int AddCE32(int ce32) // ICU4N: In Java, this was protected, but this is a sealed class
        {
            int length = ce32s.Count;
            for (int i = 0; i < length; ++i)
            {
                if (ce32 == ce32s[i]) { return i; }
            }
            ce32s.Add(ce32);
            return length;
        }

        internal int AddConditionalCE32(string context, int ce32) // ICU4N: In Java, this was protected, but this is a sealed class
        {
            Debug.Assert(context.Length != 0);
            int index = conditionalCE32s.Count;
            if (index > Collation.MAX_INDEX)
            {
                throw new IndexOutOfRangeException("too many context-sensitive mappings");
                // BufferOverflowException is a better fit
                // but cannot be constructed with a message string.
            }
            ConditionalCE32 cond = new ConditionalCE32(context, ce32);
            conditionalCE32s.Add(cond);
            return index;
        }

        private ConditionalCE32 GetConditionalCE32(int index)
        {
            return conditionalCE32s[index];
        }
        private ConditionalCE32 GetConditionalCE32ForCE32(int ce32)
        {
            return GetConditionalCE32(Collation.IndexFromCE32(ce32));
        }

        private static int MakeBuilderContextCE32(int index)
        {
            return Collation.MakeCE32FromTagAndIndex(Collation.BUILDER_DATA_TAG, index);
        }
        private static bool IsBuilderContextCE32(int ce32)
        {
            return Collation.HasCE32Tag(ce32, Collation.BUILDER_DATA_TAG);
        }

        private static int EncodeOneCEAsCE32(long ce)
        {
            long p = ce.TripleShift(32);
            int lower32 = (int)ce;
            int t = lower32 & 0xffff;
            Debug.Assert((t & 0xc000) != 0xc000);  // Impossible case bits 11 mark special CE32s.
            if ((ce & 0xffff00ff00ffL) == 0)
            {
                // normal form ppppsstt
                return (int)p | (lower32.TripleShift(16)) | (t >> 8);
            }
            else if ((ce & 0xffffffffffL) == Collation.CommonSecondaryAndTertiaryCE)
            {
                // long-primary form ppppppC1
                return Collation.MakeLongPrimaryCE32(p);
            }
            else if (p == 0 && (t & 0xff) == 0)
            {
                // long-secondary form ssssttC2
                return Collation.MakeLongSecondaryCE32(lower32);
            }
            return Collation.NO_CE32;
        }

        private int EncodeOneCE(long ce)
        {
            // Try to encode one CE as one CE32.
            int ce32 = EncodeOneCEAsCE32(ce);
            if (ce32 != Collation.NO_CE32) { return ce32; }
            int index = AddCE(ce);
            if (index > Collation.MAX_INDEX)
            {
                throw new IndexOutOfRangeException("too many mappings");
                // BufferOverflowException is a better fit
                // but cannot be constructed with a message string.
            }
            return Collation.MakeCE32FromTagIndexAndLength(Collation.EXPANSION_TAG, index, 1);
        }

        private int EncodeExpansion(IList<long> ces, int start, int length)
        {
            // See if this sequence of CEs has already been stored.
            long first = ces[start];
            int ce64sMax = ce64s.Count - length;
            for (int i = 0; i <= ce64sMax; ++i)
            {
                if (first == ce64s[i])
                {
                    if (i > Collation.MAX_INDEX)
                    {
                        throw new IndexOutOfRangeException("too many mappings");
                        // BufferOverflowException is a better fit
                        // but cannot be constructed with a message string.
                    }
                    for (int j = 1; ; ++j)
                    {
                        if (j == length)
                        {
                            return Collation.MakeCE32FromTagIndexAndLength(
                                    Collation.EXPANSION_TAG, i, length);
                        }
                        if (ce64s[i + j] != ces[start + j]) { break; }
                    }
                }
            }
            {
                // Store the new sequence.
                int i = ce64s.Count;
                if (i > Collation.MAX_INDEX)
                {
                    throw new IndexOutOfRangeException("too many mappings");
                    // BufferOverflowException is a better fit
                    // but cannot be constructed with a message string.
                }
                for (int j = 0; j < length; ++j)
                {
                    ce64s.Add(ces[start + j]);
                }
                return Collation.MakeCE32FromTagIndexAndLength(Collation.EXPANSION_TAG, i, length);
            }
        }

        private int EncodeExpansion32(IList<int> newCE32s, int start, int length)
        {
            // See if this sequence of CE32s has already been stored.
            int first = newCE32s[start];
            int ce32sMax = ce32s.Count - length;
            for (int i = 0; i <= ce32sMax; ++i)
            {
                if (first == ce32s[i])
                {
                    if (i > Collation.MAX_INDEX)
                    {
                        throw new IndexOutOfRangeException("too many mappings");
                        // BufferOverflowException is a better fit
                        // but cannot be constructed with a message string.
                    }
                    for (int j = 1; ; ++j)
                    {
                        if (j == length)
                        {
                            return Collation.MakeCE32FromTagIndexAndLength(
                                    Collation.EXPANSION32_TAG, i, length);
                        }
                        if (ce32s[i + j] != newCE32s[start + j]) { break; }
                    }
                }
            }
            {
                // Store the new sequence.
                int i = ce32s.Count;
                if (i > Collation.MAX_INDEX)
                {
                    throw new IndexOutOfRangeException("too many mappings");
                    // BufferOverflowException is a better fit
                    // but cannot be constructed with a message string.
                }
                for (int j = 0; j < length; ++j)
                {
                    ce32s.Add(newCE32s[start + j]);
                }
                return Collation.MakeCE32FromTagIndexAndLength(Collation.EXPANSION32_TAG, i, length);
            }
        }

        private int CopyFromBaseCE32(int c, int ce32, bool withContext)
        {
            if (!Collation.IsSpecialCE32(ce32)) { return ce32; }
            switch (Collation.TagFromCE32(ce32))
            {
                case Collation.LONG_PRIMARY_TAG:
                case Collation.LONG_SECONDARY_TAG:
                case Collation.LATIN_EXPANSION_TAG:
                    // copy as is
                    break;
                case Collation.EXPANSION32_TAG:
                    {
                        int index = Collation.IndexFromCE32(ce32);
                        int length = Collation.LengthFromCE32(ce32);
                        ce32 = EncodeExpansion32(base_.ce32s, index, length);
                        break;
                    }
                case Collation.EXPANSION_TAG:
                    {
                        int index = Collation.IndexFromCE32(ce32);
                        int length = Collation.LengthFromCE32(ce32);
                        ce32 = EncodeExpansion(base_.ces, index, length);
                        break;
                    }
                case Collation.PREFIX_TAG:
                    {
                        // Flatten prefixes and nested suffixes (contractions)
                        // into a linear list of ConditionalCE32.
                        int trieIndex = Collation.IndexFromCE32(ce32);
                        ce32 = base_.GetCE32FromContexts(trieIndex);  // Default if no prefix match.
                        if (!withContext)
                        {
                            return CopyFromBaseCE32(c, ce32, false);
                        }
                        ConditionalCE32 head = new ConditionalCE32("", 0);
                        StringBuilder context = new StringBuilder("\0");
                        int index;
                        if (Collation.IsContractionCE32(ce32))
                        {
                            index = CopyContractionsFromBaseCE32(context, c, ce32, head);
                        }
                        else
                        {
                            ce32 = CopyFromBaseCE32(c, ce32, true);
                            head.Next = index = AddConditionalCE32(context.ToString(), ce32);
                        }
                        ConditionalCE32 cond = GetConditionalCE32(index);  // the last ConditionalCE32 so far
                        using (CharsTrieEnumerator prefixes = CharsTrie.GetEnumerator(base_.contexts, trieIndex + 2, 0))
                        {
                            while (prefixes.MoveNext())
                            {
                                CharsTrieEntry entry = prefixes.Current;
                                context.Length = 0;
                                context.Append(entry.Chars).Reverse().Insert(0, (char)entry.Chars.Length);
                                ce32 = entry.Value;
                                if (Collation.IsContractionCE32(ce32))
                                {
                                    index = CopyContractionsFromBaseCE32(context, c, ce32, cond);
                                }
                                else
                                {
                                    ce32 = CopyFromBaseCE32(c, ce32, true);
                                    cond.Next = index = AddConditionalCE32(context.ToString(), ce32);
                                }
                                cond = GetConditionalCE32(index);
                            }
                        }
                        ce32 = MakeBuilderContextCE32(head.Next);
                        contextChars.Add(c);
                        break;
                    }
                case Collation.CONTRACTION_TAG:
                    {
                        if (!withContext)
                        {
                            int index = Collation.IndexFromCE32(ce32);
                            ce32 = base_.GetCE32FromContexts(index);  // Default if no suffix match.
                            return CopyFromBaseCE32(c, ce32, false);
                        }
                        ConditionalCE32 head = new ConditionalCE32("", 0);
                        StringBuilder context = new StringBuilder("\0");
                        CopyContractionsFromBaseCE32(context, c, ce32, head);
                        ce32 = MakeBuilderContextCE32(head.Next);
                        contextChars.Add(c);
                        break;
                    }
                case Collation.HANGUL_TAG:
                    throw new NotSupportedException("We forbid tailoring of Hangul syllables.");
                case Collation.OFFSET_TAG:
                    ce32 = GetCE32FromOffsetCE32(true, c, ce32);
                    break;
                case Collation.IMPLICIT_TAG:
                    ce32 = EncodeOneCE(Collation.UnassignedCEFromCodePoint(c));
                    break;
                default:
                    throw new InvalidOperationException("CopyFromBaseCE32(c, ce32, withContext) " +
                            "requires ce32 == base_.GetFinalCE32(ce32)");
            }
            return ce32;
        }

        /// <summary>
        /// Copies base contractions to a list of <see cref="ConditionalCE32"/>.
        /// Sets <c>cond.Next</c> to the index of the first new item
        /// and returns the index of the last new item.
        /// </summary>
        private int CopyContractionsFromBaseCE32(StringBuilder context, int c, int ce32,
                ConditionalCE32 cond)
        {
            int trieIndex = Collation.IndexFromCE32(ce32);
            int index;
            if ((ce32 & Collation.CONTRACT_SINGLE_CP_NO_MATCH) != 0)
            {
                // No match on the single code point.
                // We are underneath a prefix, and the default mapping is just
                // a fallback to the mappings for a shorter prefix.
                Debug.Assert(context.Length > 1);
                index = -1;
            }
            else
            {
                ce32 = base_.GetCE32FromContexts(trieIndex);  // Default if no suffix match.
                Debug.Assert(!Collation.IsContractionCE32(ce32));
                ce32 = CopyFromBaseCE32(c, ce32, true);
                cond.Next = index = AddConditionalCE32(context.ToString(), ce32);
                cond = GetConditionalCE32(index);
            }

            int suffixStart = context.Length;
            using (CharsTrieEnumerator suffixes = CharsTrie.GetEnumerator(base_.contexts, trieIndex + 2, 0))
            {
                while (suffixes.MoveNext())
                {
                    CharsTrieEntry entry = suffixes.Current;
                    context.Append(entry.Chars);
                    ce32 = CopyFromBaseCE32(c, entry.Value, true);
                    cond.Next = index = AddConditionalCE32(context.ToString(), ce32);
                    // No need to update the unsafeBackwardSet because the tailoring set
                    // is already a copy of the base set.
                    cond = GetConditionalCE32(index);
                    context.Length = suffixStart;
                }
            }
            Debug.Assert(index >= 0);
            return index;
        }

        private sealed class CopyHelper
        {
            internal CopyHelper(CollationDataBuilder s, CollationDataBuilder d,
                      CollationDataBuilder.ICEModifier m)
            {
                src = s;
                dest = d;
                modifier = m;
            }

            internal void CopyRangeCE32(int start, int end, int ce32)
            {
                ce32 = CopyCE32(ce32);
                dest.trie.SetRange(start, end, ce32, true);
                if (CollationDataBuilder.IsBuilderContextCE32(ce32))
                {
                    dest.contextChars.Add(start, end);
                }
            }

            internal int CopyCE32(int ce32)
            {
                if (!Collation.IsSpecialCE32(ce32))
                {
                    long ce = modifier.ModifyCE32(ce32);
                    if (ce != Collation.NoCE)
                    {
                        ce32 = dest.EncodeOneCE(ce);
                    }
                }
                else
                {
                    int tag = Collation.TagFromCE32(ce32);
                    if (tag == Collation.EXPANSION32_TAG)
                    {
                        IList<int> srcCE32s = src.ce32s;
                        int srcIndex = Collation.IndexFromCE32(ce32);
                        int length = Collation.LengthFromCE32(ce32);
                        // Inspect the source CE32s. Just copy them if none are modified.
                        // Otherwise copy to modifiedCEs, with modifications.
                        bool isModified = false;
                        for (int i = 0; i < length; ++i)
                        {
                            ce32 = srcCE32s[srcIndex + i];
                            long ce;
                            if (Collation.IsSpecialCE32(ce32) ||
                                    (ce = modifier.ModifyCE32(ce32)) == Collation.NoCE)
                            {
                                if (isModified)
                                {
                                    modifiedCEs[i] = Collation.CeFromCE32(ce32);
                                }
                            }
                            else
                            {
                                if (!isModified)
                                {
                                    for (int j = 0; j < i; ++j)
                                    {
                                        modifiedCEs[j] = Collation.CeFromCE32(srcCE32s[srcIndex + j]);
                                    }
                                    isModified = true;
                                }
                                modifiedCEs[i] = ce;
                            }
                        }
                        if (isModified)
                        {
                            ce32 = dest.EncodeCEs(modifiedCEs, length);
                        }
                        else
                        {
                            ce32 = dest.EncodeExpansion32(srcCE32s, srcIndex, length);
                        }
                    }
                    else if (tag == Collation.EXPANSION_TAG)
                    {
                        IList<long> srcCEs = src.ce64s;
                        int srcIndex = Collation.IndexFromCE32(ce32);
                        int length = Collation.LengthFromCE32(ce32);
                        // Inspect the source CEs. Just copy them if none are modified.
                        // Otherwise copy to modifiedCEs, with modifications.
                        bool isModified = false;
                        for (int i = 0; i < length; ++i)
                        {
                            long srcCE = srcCEs[srcIndex + i];
                            long ce = modifier.ModifyCE(srcCE);
                            if (ce == Collation.NoCE)
                            {
                                if (isModified)
                                {
                                    modifiedCEs[i] = srcCE;
                                }
                            }
                            else
                            {
                                if (!isModified)
                                {
                                    for (int j = 0; j < i; ++j)
                                    {
                                        modifiedCEs[j] = srcCEs[srcIndex + j];
                                    }
                                    isModified = true;
                                }
                                modifiedCEs[i] = ce;
                            }
                        }
                        if (isModified)
                        {
                            ce32 = dest.EncodeCEs(modifiedCEs, length);
                        }
                        else
                        {
                            ce32 = dest.EncodeExpansion(srcCEs, srcIndex, length);
                        }
                    }
                    else if (tag == Collation.BUILDER_DATA_TAG)
                    {
                        // Copy the list of ConditionalCE32.
                        ConditionalCE32 cond = src.GetConditionalCE32ForCE32(ce32);
                        Debug.Assert(!cond.HasContext);
                        int destIndex = dest.AddConditionalCE32(
                                cond.Context, CopyCE32(cond.Ce32));
                        ce32 = CollationDataBuilder.MakeBuilderContextCE32(destIndex);
                        while (cond.Next >= 0)
                        {
                            cond = src.GetConditionalCE32(cond.Next);
                            ConditionalCE32 prevDestCond = dest.GetConditionalCE32(destIndex);
                            destIndex = dest.AddConditionalCE32(
                                    cond.Context, CopyCE32(cond.Ce32));
                            int suffixStart = cond.PrefixLength + 1;
                            dest.unsafeBackwardSet.AddAll(cond.Context.Substring(suffixStart));
                            prevDestCond.Next = destIndex;
                        }
                    }
                    else
                    {
                        // Just copy long CEs and Latin mini expansions (and other expected values) as is,
                        // assuming that the modifier would not modify them.
                        Debug.Assert(tag == Collation.LONG_PRIMARY_TAG ||
                                tag == Collation.LONG_SECONDARY_TAG ||
                                tag == Collation.LATIN_EXPANSION_TAG ||
                                tag == Collation.HANGUL_TAG);
                    }
                }
                return ce32;
            }

            CollationDataBuilder src;
            CollationDataBuilder dest;
            CollationDataBuilder.ICEModifier modifier;
            long[] modifiedCEs = new long[Collation.MAX_EXPANSION_LENGTH];
        }

        private static void EnumRangeForCopy(int start, int end, int value, CopyHelper helper)
        {
            if (value != Collation.UNASSIGNED_CE32 && value != Collation.FALLBACK_CE32)
            {
                helper.CopyRangeCE32(start, end, value);
            }
        }

        private bool GetJamoCE32s(int[] jamoCE32s)
        {
            bool anyJamoAssigned = base_ == null;  // always set jamoCE32s in the base data
            bool needToCopyFromBase = false;
            for (int j = 0; j < CollationData.JAMO_CE32S_LENGTH; ++j)
            {  // Count across Jamo types.
                int jamo = JamoCpFromIndex(j);
                bool fromBase = false;
                int ce32 = trie.Get(jamo);
                anyJamoAssigned |= Collation.IsAssignedCE32(ce32);
                // TODO: Try to prevent [optimize [Jamo]] from counting as anyJamoAssigned.
                // (As of CLDR 24 [2013] the Korean tailoring does not optimize conjoining Jamo.)
                if (ce32 == Collation.FALLBACK_CE32)
                {
                    fromBase = true;
                    ce32 = base_.GetCE32(jamo);
                }
                if (Collation.IsSpecialCE32(ce32))
                {
                    switch (Collation.TagFromCE32(ce32))
                    {
                        case Collation.LONG_PRIMARY_TAG:
                        case Collation.LONG_SECONDARY_TAG:
                        case Collation.LATIN_EXPANSION_TAG:
                            // Copy the ce32 as-is.
                            break;
                        case Collation.EXPANSION32_TAG:
                        case Collation.EXPANSION_TAG:
                        case Collation.PREFIX_TAG:
                        case Collation.CONTRACTION_TAG:
                            if (fromBase)
                            {
                                // Defer copying until we know if anyJamoAssigned.
                                ce32 = Collation.FALLBACK_CE32;
                                needToCopyFromBase = true;
                            }
                            break;
                        case Collation.IMPLICIT_TAG:
                            // An unassigned Jamo should only occur in tests with incomplete bases.
                            Debug.Assert(fromBase);
                            ce32 = Collation.FALLBACK_CE32;
                            needToCopyFromBase = true;
                            break;
                        case Collation.OFFSET_TAG:
                            ce32 = GetCE32FromOffsetCE32(fromBase, jamo, ce32);
                            break;
                        case Collation.FALLBACK_TAG:
                        case Collation.RESERVED_TAG_3:
                        case Collation.BUILDER_DATA_TAG:
                        case Collation.DIGIT_TAG:
                        case Collation.U0000_TAG:
                        case Collation.HANGUL_TAG:
                        case Collation.LEAD_SURROGATE_TAG:
                            throw new InvalidOperationException(string.Format("unexpected special tag in ce32=0x%08x", ce32));
                    }
                }
                jamoCE32s[j] = ce32;
            }
            if (anyJamoAssigned && needToCopyFromBase)
            {
                for (int j = 0; j < CollationData.JAMO_CE32S_LENGTH; ++j)
                {
                    if (jamoCE32s[j] == Collation.FALLBACK_CE32)
                    {
                        int jamo = JamoCpFromIndex(j);
                        jamoCE32s[j] = CopyFromBaseCE32(jamo, base_.GetCE32(jamo),
                                                        /*withContext=*/ true);
                    }
                }
            }
            return anyJamoAssigned;
        }

        private void SetDigitTags()
        {
            UnicodeSet digits = new UnicodeSet("[:Nd:]");
            UnicodeSetIterator iter = new UnicodeSetIterator(digits);
            while (iter.Next())
            {
                Debug.Assert(iter.Codepoint != UnicodeSetIterator.IsString);
                int c = iter.Codepoint;
                int ce32 = trie.Get(c);
                if (ce32 != Collation.FALLBACK_CE32 && ce32 != Collation.UNASSIGNED_CE32)
                {
                    int index = AddCE32(ce32);
                    if (index > Collation.MAX_INDEX)
                    {
                        throw new IndexOutOfRangeException("too many mappings");
                        // BufferOverflowException is a better fit
                        // but cannot be constructed with a message string.
                    }
                    ce32 = Collation.MakeCE32FromTagIndexAndLength(
                            Collation.DIGIT_TAG, index, UChar.Digit(c));  // u_charDigitValue(c)
                    trie.Set(c, ce32);
                }
            }
        }

        private void SetLeadSurrogates()
        {
            for (char lead = (char)0xd800; lead < 0xdc00; ++lead)
            {
                int leadValue = -1;
                // utrie2_enumForLeadSurrogate(trie, lead, null, , &value);
                IEnumerator<Trie2Range> trieIterator = trie.GetEnumeratorForLeadSurrogate(lead);
                while (trieIterator.MoveNext())
                {
                    Trie2Range range = trieIterator.Current;
                    // The rest of this loop is equivalent to C++ enumRangeLeadValue().
                    int value = range.Value;
                    if (value == Collation.UNASSIGNED_CE32)
                    {
                        value = Collation.LEAD_ALL_UNASSIGNED;
                    }
                    else if (value == Collation.FALLBACK_CE32)
                    {
                        value = Collation.LEAD_ALL_FALLBACK;
                    }
                    else
                    {
                        leadValue = Collation.LEAD_MIXED;
                        break;
                    }
                    if (leadValue < 0)
                    {
                        leadValue = value;
                    }
                    else if (leadValue != value)
                    {
                        leadValue = Collation.LEAD_MIXED;
                        break;
                    }
                }
                trie.SetForLeadSurrogateCodeUnit(lead,
                        Collation.MakeCE32FromTagAndIndex(Collation.LEAD_SURROGATE_TAG, 0) | leadValue);
            }
        }

        private void BuildMappings(CollationData data)
        {
            if (!IsMutable)
            {
                throw new InvalidOperationException("attempt to build() after build()");
            }

            BuildContexts();

            int[] jamoCE32s = new int[CollationData.JAMO_CE32S_LENGTH];
            int jamoIndex = -1;
            if (GetJamoCE32s(jamoCE32s))
            {
                jamoIndex = ce32s.Count;
                for (int i = 0; i < CollationData.JAMO_CE32S_LENGTH; ++i)
                {
                    ce32s.Add(jamoCE32s[i]);
                }
                // Small optimization: Use a bit in the Hangul ce32
                // to indicate that none of the Jamo CE32s are isSpecialCE32()
                // (as it should be in the root collator).
                // It allows CollationIterator to avoid recursive function calls and per-Jamo tests.
                // In order to still have good trie compression and keep this code simple,
                // we only set this flag if a whole block of 588 Hangul syllables starting with
                // a common leading consonant (Jamo L) has this property.
                bool isAnyJamoVTSpecial = false;
                for (int i = Hangul.JamoLCount; i < CollationData.JAMO_CE32S_LENGTH; ++i)
                {
                    if (Collation.IsSpecialCE32(jamoCE32s[i]))
                    {
                        isAnyJamoVTSpecial = true;
                        break;
                    }
                }
                int hangulCE32 = Collation.MakeCE32FromTagAndIndex(Collation.HANGUL_TAG, 0);
                int c = Hangul.HangulBase;
                for (int i = 0; i < Hangul.JamoLCount; ++i)
                {  // iterate over the Jamo L
                    int ce32 = hangulCE32;
                    if (!isAnyJamoVTSpecial && !Collation.IsSpecialCE32(jamoCE32s[i]))
                    {
                        ce32 |= Collation.HANGUL_NO_SPECIAL_JAMO;
                    }
                    int limit = c + Hangul.JamoVTCount;
                    trie.SetRange(c, limit - 1, ce32, true);
                    c = limit;
                }
            }
            else
            {
                // Copy the Hangul CE32s from the base in blocks per Jamo L,
                // assuming that HANGUL_NO_SPECIAL_JAMO is set or not set for whole blocks.
                for (int c = Hangul.HangulBase; c < Hangul.HangulLimit;)
                {
                    int ce32 = base_.GetCE32(c);
                    Debug.Assert(Collation.HasCE32Tag(ce32, Collation.HANGUL_TAG));
                    int limit = c + Hangul.JamoVTCount;
                    trie.SetRange(c, limit - 1, ce32, true);
                    c = limit;
                }
            }

            SetDigitTags();
            SetLeadSurrogates();

            // For U+0000, move its normal ce32 into CE32s[0] and set U0000_TAG.
            ce32s[0] = trie.Get(0);
            trie.Set(0, Collation.MakeCE32FromTagAndIndex(Collation.U0000_TAG, 0));

            data.trie = trie.ToTrie2_32();

            {
                // Mark each lead surrogate as "unsafe"
                // if any of its 1024 associated supplementary code points is "unsafe".
                int c = 0x10000;
                for (char lead = (char)0xd800; lead < 0xdc00; ++lead, c += 0x400)
                {
                    if (unsafeBackwardSet.ContainsSome(c, c + 0x3ff))
                    {
                        unsafeBackwardSet.Add(lead);
                    }
                }
                unsafeBackwardSet.Freeze();

                data.ce32s = ce32s;
                data.ces = ce64s;
                data.contexts = contexts.ToString();

                data.Base = base_;
                if (jamoIndex >= 0)
                {
                    data.jamoCE32s = jamoCE32s;  // C++: data.ce32s + jamoIndex
                }
                else
                {
                    data.jamoCE32s = base_.jamoCE32s;
                }
                data.unsafeBackwardSet = unsafeBackwardSet;
            }
        }

        private void ClearContexts()
        {
            contexts.Length = 0;
            UnicodeSetIterator iter = new UnicodeSetIterator(contextChars);
            while (iter.Next())
            {
                Debug.Assert(iter.Codepoint != UnicodeSetIterator.IsString);
                int ce32 = trie.Get(iter.Codepoint);
                Debug.Assert(IsBuilderContextCE32(ce32));
                GetConditionalCE32ForCE32(ce32).BuiltCE32 = Collation.NO_CE32;
            }
        }

        private void BuildContexts()
        {
            // Ignore abandoned lists and the cached builtCE32,
            // and build all contexts from scratch.
            contexts.Length = 0;
            UnicodeSetIterator iter = new UnicodeSetIterator(contextChars);
            while (iter.Next())
            {
                Debug.Assert(iter.Codepoint != UnicodeSetIterator.IsString);
                int c = iter.Codepoint;
                int ce32 = trie.Get(c);
                if (!IsBuilderContextCE32(ce32))
                {
                    throw new InvalidOperationException("Impossible: No context data for c in contextChars.");
                }
                ConditionalCE32 cond = GetConditionalCE32ForCE32(ce32);
                ce32 = BuildContext(cond);
                trie.Set(c, ce32);
            }
        }

        private int BuildContext(ConditionalCE32 head)
        {
            // The list head must have no context.
            Debug.Assert(!head.HasContext);
            // The list head must be followed by one or more nodes that all do have context.
            Debug.Assert(head.Next >= 0);
            CharsTrieBuilder prefixBuilder = new CharsTrieBuilder();
            CharsTrieBuilder contractionBuilder = new CharsTrieBuilder();
            for (ConditionalCE32 cond = head; ; cond = GetConditionalCE32(cond.Next))
            {
                // After the list head, the prefix or suffix can be empty, but not both.
                Debug.Assert(cond == head || cond.HasContext);
                int prefixLength = cond.PrefixLength;
                StringBuilder prefix = new StringBuilder().Append(cond.Context, 0, (prefixLength + 1) - 0); // ICU4N: Checked 3rd parameter
                string prefixString = prefix.ToString();
                // Collect all contraction suffixes for one prefix.
                ConditionalCE32 firstCond = cond;
                ConditionalCE32 lastCond = cond;
                while (cond.Next >= 0 &&
                        (cond = GetConditionalCE32(cond.Next)).Context.StartsWith(prefixString, StringComparison.Ordinal))
                {
                    lastCond = cond;
                }
                int ce32;
                int suffixStart = prefixLength + 1;  // == prefix.length()
                if (lastCond.Context.Length == suffixStart)
                {
                    // One prefix without contraction suffix.
                    Debug.Assert(firstCond == lastCond);
                    ce32 = lastCond.Ce32;
                    cond = lastCond;
                }
                else
                {
                    // Build the contractions trie.
                    contractionBuilder.Clear();
                    // Entry for an empty suffix, to be stored before the trie.
                    int emptySuffixCE32 = Collation.NO_CE32;  // Will always be set to a real value.
                    int flags = 0;
                    if (firstCond.Context.Length == suffixStart)
                    {
                        // There is a mapping for the prefix and the single character c. (p|c)
                        // If no other suffix matches, then we return this value.
                        emptySuffixCE32 = firstCond.Ce32;
                        cond = GetConditionalCE32(firstCond.Next);
                    }
                    else
                    {
                        // There is no mapping for the prefix and just the single character.
                        // (There is no p|c, only p|cd, p|ce etc.)
                        flags |= Collation.CONTRACT_SINGLE_CP_NO_MATCH;
                        // When the prefix matches but none of the prefix-specific suffixes,
                        // then we fall back to the mappings with the next-longest prefix,
                        // and ultimately to mappings with no prefix.
                        // Each fallback might be another set of contractions.
                        // For example, if there are mappings for ch, p|cd, p|ce, but not for p|c,
                        // then in text "pch" we find the ch contraction.
                        for (cond = head; ; cond = GetConditionalCE32(cond.Next))
                        {
                            int length = cond.PrefixLength;
                            if (length == prefixLength) { break; }
                            if (cond.DefaultCE32 != Collation.NO_CE32 &&
                                    (length == 0 || prefixString.RegionMatches(
                                            prefix.Length - length, cond.Context, 1, length, StringComparison.Ordinal)
                                            /* C++: prefix.endsWith(cond.context, 1, length) */))
                            {
                                emptySuffixCE32 = cond.DefaultCE32;
                            }
                        }
                        cond = firstCond;
                    }
                    // Optimization: Set a flag when
                    // the first character of every contraction suffix has lccc!=0.
                    // Short-circuits contraction matching when a normal letter follows.
                    flags |= Collation.CONTRACT_NEXT_CCC;
                    // Add all of the non-empty suffixes into the contraction trie.
                    for (; ; )
                    {
                        string suffix = cond.Context.Substring(suffixStart);
                        int fcd16 = nfcImpl.GetFCD16(suffix.CodePointAt(0));
                        if (fcd16 <= 0xff)
                        {
                            flags &= ~Collation.CONTRACT_NEXT_CCC;
                        }
                        fcd16 = nfcImpl.GetFCD16(suffix.CodePointBefore(suffix.Length));
                        if (fcd16 > 0xff)
                        {
                            // The last suffix character has lccc!=0, allowing for discontiguous contractions.
                            flags |= Collation.CONTRACT_TRAILING_CCC;
                        }
                        contractionBuilder.Add(suffix, cond.Ce32);
                        if (cond == lastCond) { break; }
                        cond = GetConditionalCE32(cond.Next);
                    }
                    int index = AddContextTrie(emptySuffixCE32, contractionBuilder);
                    if (index > Collation.MAX_INDEX)
                    {
                        throw new IndexOutOfRangeException("too many context-sensitive mappings");
                        // BufferOverflowException is a better fit
                        // but cannot be constructed with a message string.
                    }
                    ce32 = Collation.MakeCE32FromTagAndIndex(Collation.CONTRACTION_TAG, index) | flags;
                }
                Debug.Assert(cond == lastCond);
                firstCond.DefaultCE32 = ce32;
                if (prefixLength == 0)
                {
                    if (cond.Next < 0)
                    {
                        // No non-empty prefixes, only contractions.
                        return ce32;
                    }
                }
                else
                {
                    prefix.Delete(0, 1 - 0);  // Remove the length unit. // ICU4N: Corrected 2nd parameter
                    prefix.Reverse();
                    prefixBuilder.Add(prefix, ce32);
                    if (cond.Next < 0) { break; }
                }
            }

            {
                Debug.Assert(head.DefaultCE32 != Collation.NO_CE32);
                int index = AddContextTrie(head.DefaultCE32, prefixBuilder);
                if (index > Collation.MAX_INDEX)
                {
                    throw new IndexOutOfRangeException("too many context-sensitive mappings");
                    // BufferOverflowException is a better fit
                    // but cannot be constructed with a message string.
                }
                return Collation.MakeCE32FromTagAndIndex(Collation.PREFIX_TAG, index);
            }
        }

        private int AddContextTrie(int defaultCE32, CharsTrieBuilder trieBuilder)
        {
            StringBuilder context = new StringBuilder();
            context.Append((char)(defaultCE32 >> 16)).Append((char)defaultCE32);
            context.Append(trieBuilder.BuildCharSequence(TrieBuilderOption.Small));
            // ICU4N: IndexOf method on StringBuilder is extremely slow, so we call ToString() first,
            // which generally gets better performance.
            int index = contexts.ToString().IndexOf(context.ToString(), StringComparison.Ordinal);
            if (index < 0)
            {
                index = contexts.Length;
                contexts.Append(context);
            }
            return index;
        }

        private void BuildFastLatinTable(CollationData data)
        {
            if (!fastLatinEnabled) { return; }

            fastLatinBuilder = new CollationFastLatinBuilder();
            if (fastLatinBuilder.ForData(data))
            {
                char[] header = fastLatinBuilder.GetHeader();
                char[] table = fastLatinBuilder.GetTable();
                if (base_ != null &&
                        ArrayEqualityComparer<char>.OneDimensional.Equals(header, base_.fastLatinTableHeader) &&
                        ArrayEqualityComparer<char>.OneDimensional.Equals(table, base_.FastLatinTable))
                {
                    // Same fast Latin table as in the base, use that one instead.
                    fastLatinBuilder = null;
                    header = base_.fastLatinTableHeader;
                    table = base_.FastLatinTable;
                }
                data.fastLatinTableHeader = header;
                data.FastLatinTable = table;
            }
            else
            {
                fastLatinBuilder = null;
            }
        }

        private int GetCEs(ICharSequence s, int start, long[] ces, int cesLength)
        {
            if (collIter == null)
            {
                collIter = new DataBuilderCollationIterator(this, new CollationData(nfcImpl));
                if (collIter == null) { return 0; }
            }
            return collIter.FetchCEs(s, start, ces, cesLength);
        }

        private static int JamoCpFromIndex(int i)
        {
            // 0 <= i < CollationData.JAMO_CE32S_LENGTH = 19 + 21 + 27
            if (i < Hangul.JamoLCount) { return Hangul.JamoLBase + i; }
            i -= Hangul.JamoLCount;
            if (i < Hangul.JamoVCount) { return Hangul.JamoVBase + i; }
            i -= Hangul.JamoVCount;
            // i < 27
            return Hangul.JamoTBase + 1 + i;
        }

        /// <summary>
        /// Build-time collation element and character iterator.
        /// Uses the runtime <see cref="CollationIterator"/> for fetching CEs for a string
        /// but reads from the builder's unfinished data structures.
        /// <para/>
        /// In particular, this class reads from the unfinished trie
        /// and has to avoid <see cref="CollationIterator.NextCE()"/> and redirect other
        /// calls to data.GetCE32() and data.GetCE32FromSupplementary().
        /// <para/>
        /// We do this so that we need not implement the collation algorithm
        /// again for the builder and make it behave exactly like the runtime code.
        /// That would be more difficult to test and maintain than this indirection.
        /// Some CE32 tags (for example, the DIGIT_TAG) do not occur in the builder data,
        /// so the data accesses from those code paths need not be modified.
        /// <para/>
        /// This class iterates directly over whole code points
        /// so that the <see cref="CollationIterator"/> does not need the finished trie
        /// for handling the LEAD_SURROGATE_TAG.
        /// </summary>
        private sealed class DataBuilderCollationIterator : CollationIterator
        {
            internal DataBuilderCollationIterator(CollationDataBuilder b, CollationData newData)
            : base(newData, /*numeric=*/ false)
            {
                builder = b;
                builderData = newData;
                builderData.Base = builder.base_;
                // Set all of the jamoCE32s[] to indirection CE32s.
                for (int j = 0; j < CollationData.JAMO_CE32S_LENGTH; ++j)
                {  // Count across Jamo types.
                    int jamo = CollationDataBuilder.JamoCpFromIndex(j);
                    jamoCE32s[j] = Collation.MakeCE32FromTagAndIndex(Collation.BUILDER_DATA_TAG, jamo) |
                                    CollationDataBuilder.IS_BUILDER_JAMO_CE32;
                }
                builderData.jamoCE32s = jamoCE32s;
            }

            internal int FetchCEs(ICharSequence str, int start, long[] ces, int cesLength)
            {
                // Set the pointers each time, in case they changed due to reallocation.
                builderData.ce32s = builder.ce32s;
                builderData.ces = builder.ce64s;
                builderData.contexts = builder.contexts.ToString();
                // Modified copy of CollationIterator.nextCE() and CollationIterator.nextCEFromCE32().
                Reset();
                s = str;
                pos = start;
                while (pos < s.Length)
                {
                    // No need to keep all CEs in the iterator buffer.
                    ClearCEs();
                    int c = Character.CodePointAt(s, pos);
                    pos += Character.CharCount(c);
                    int ce32 = builder.trie.Get(c);
                    CollationData d;
                    if (ce32 == Collation.FALLBACK_CE32)
                    {
                        d = builder.base_;
                        ce32 = builder.base_.GetCE32(c);
                    }
                    else
                    {
                        d = builderData;
                    }
                    AppendCEsFromCE32(d, c, ce32, /*forward=*/ true);
                    for (int i = 0; i < CEsLength; ++i)
                    {
                        long ce = GetCE(i);
                        if (ce != 0)
                        {
                            if (cesLength < Collation.MAX_EXPANSION_LENGTH)
                            {
                                ces[cesLength] = ce;
                            }
                            ++cesLength;
                        }
                    }
                }
                return cesLength;
            }

            public override void ResetToOffset(int newOffset)
            {
                Reset();
                pos = newOffset;
            }

            public override int Offset => pos;

            public override int NextCodePoint()
            {
                if (pos == s.Length)
                {
                    return Collation.SentinelCodePoint;
                }
                int c = Character.CodePointAt(s, pos);
                pos += Character.CharCount(c);
                return c;
            }

            public override int PreviousCodePoint()
            {
                if (pos == 0)
                {
                    return Collation.SentinelCodePoint;
                }
                int c = Character.CodePointBefore(s, pos);
                pos -= Character.CharCount(c);
                return c;
            }

            protected override void ForwardNumCodePoints(int num)
            {
                pos = Character.OffsetByCodePoints(s, pos, num);
            }

            protected override void BackwardNumCodePoints(int num)
            {
                pos = Character.OffsetByCodePoints(s, pos, -num);
            }

            protected override int GetDataCE32(int c)
            {
                return builder.trie.Get(c);
            }

            protected override int GetCE32FromBuilderData(int ce32)
            {
                Debug.Assert(Collation.HasCE32Tag(ce32, Collation.BUILDER_DATA_TAG));
                if ((ce32 & CollationDataBuilder.IS_BUILDER_JAMO_CE32) != 0)
                {
                    int jamo = Collation.IndexFromCE32(ce32);
                    return builder.trie.Get(jamo);
                }
                else
                {
                    ConditionalCE32 cond = builder.GetConditionalCE32ForCE32(ce32);
                    if (cond.BuiltCE32 == Collation.NO_CE32)
                    {
                        // Build the context-sensitive mappings into their runtime form and cache the result.
                        try
                        {
                            cond.BuiltCE32 = builder.BuildContext(cond);
                        }
                        catch (IndexOutOfRangeException) // ICU4N TODO: Try to factor out this exception
                        {
                            builder.ClearContexts();
                            cond.BuiltCE32 = builder.BuildContext(cond);
                        }
                        builderData.contexts = builder.contexts.ToString();
                    }
                    return cond.BuiltCE32;
                }
            }

            private readonly CollationDataBuilder builder;
            private readonly CollationData builderData;
            private readonly int[] jamoCE32s = new int[CollationData.JAMO_CE32S_LENGTH];
            private ICharSequence s;
            private int pos;
        }

        // C++ tests !(trie == NULL || utrie2_isFrozen(trie))
        // but Java Trie2Writable does not have an observable isFrozen() state.
        internal bool IsMutable // ICU4N: In Java, this was protected, but this is a sealed class
            => trie != null && unsafeBackwardSet != null && !unsafeBackwardSet.IsFrozen;

        /// <summary>
        /// <see cref="Collation.BUILDER_DATA_TAG"/>
        /// </summary>
        private const int IS_BUILDER_JAMO_CE32 = 0x100;

        private Normalizer2Impl nfcImpl;
        private CollationData base_;
        //private CollationSettings baseSettings; // ICU4N specific - not used
        private Trie2Writable trie;
        private IList<int> ce32s;
        private IList<long> ce64s;
        private List<ConditionalCE32> conditionalCE32s;  // vector of ConditionalCE32
                                                         // Characters that have context (prefixes or contraction suffixes).
        private UnicodeSet contextChars = new UnicodeSet();
        // Serialized UCharsTrie structures for finalized contexts.
        private StringBuilder contexts = new StringBuilder();
        private UnicodeSet unsafeBackwardSet = new UnicodeSet();
        private bool modified;

        private bool fastLatinEnabled;
        private CollationFastLatinBuilder fastLatinBuilder;

        private DataBuilderCollationIterator collIter;
    }
}
