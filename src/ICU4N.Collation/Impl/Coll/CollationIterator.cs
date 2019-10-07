using ICU4N.Support.Text;
using ICU4N.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Hangul = ICU4N.Impl.Normalizer2Impl.Hangul;

namespace ICU4N.Impl.Coll
{
    // CollationIterator.cs, ported from collationiterator.h/.cpp
    //
    // C++ version created on: 2010oct27
    // created by: Markus W. Scherer

    /// <summary>
    /// Collation element iterator and abstract character iterator.
    /// <para/>
    /// When a method returns a code point value, it must be in 0..10FFFF,
    /// except it can be negative as a sentinel value.
    /// </summary>
    public abstract class CollationIterator
    {
        private sealed class CEBuffer
        {
            /// <summary>Large enough for CEs of most short strings.</summary>
            private const int INITIAL_CAPACITY = 40;

            internal CEBuffer() { }

            internal void Append(long ce)
            {
                if (length >= INITIAL_CAPACITY)
                {
                    EnsureAppendCapacity(1);
                }
                buffer[length++] = ce;
            }

            internal void AppendUnsafe(long ce)
            {
                buffer[length++] = ce;
            }

            internal void EnsureAppendCapacity(int appCap)
            {
                int capacity = buffer.Length;
                if ((length + appCap) <= capacity) { return; }
                do
                {
                    if (capacity < 1000)
                    {
                        capacity *= 4;
                    }
                    else
                    {
                        capacity *= 2;
                    }
                } while (capacity < (length + appCap));
                long[] newBuffer = new long[capacity];
                System.Array.Copy(buffer, 0, newBuffer, 0, length);
                buffer = newBuffer;
            }

            internal void IncLength()
            {
                // Use INITIAL_CAPACITY for a very simple fastpath.
                // (Rather than buffer.getCapacity().)
                if (length >= INITIAL_CAPACITY)
                {
                    EnsureAppendCapacity(1);
                }
                ++length;
            }

            internal long this[int index] // ICU4N specific - converted from Get() and Set() methods in Java to .NET indexer
            {
                get { return buffer[index]; }
                set { buffer[index] = value; }
            }

            internal long[] GetCEs() { return buffer; }

            private int length = 0;
            public int Length
            {
                get { return length; }
                internal set { length = value; }
            }

            private long[] buffer = new long[INITIAL_CAPACITY];
        }

        // State of combining marks skipped in discontiguous contraction.
        // We create a state object on first use and keep it around deactivated between uses.
        private sealed class SkippedState
        {
            // Born active but empty.
            internal SkippedState() { }
            internal void Clear()
            {
                oldBuffer.Length = 0;
                pos = 0;
                // The newBuffer is reset by setFirstSkipped().
            }

            internal bool IsEmpty { get { return oldBuffer.Length == 0; } }

            internal bool HasNext { get { return pos < oldBuffer.Length; } }

            // Requires hasNext().
            internal int Next()
            {
                int c = oldBuffer.CodePointAt(pos);
                pos += Character.CharCount(c);
                return c;
            }

            // Accounts for one more input code point read beyond the end of the marks buffer.
            internal void IncBeyond()
            {
                Debug.Assert(!HasNext);
                ++pos;
            }

            // Goes backward through the skipped-marks buffer.
            // Returns the number of code points read beyond the skipped marks
            // that need to be backtracked through normal input.
            internal int BackwardNumCodePoints(int n)
            {
                int length = oldBuffer.Length;
                int beyond = pos - length;
                if (beyond > 0)
                {
                    if (beyond >= n)
                    {
                        // Not back far enough to re-enter the oldBuffer.
                        pos -= n;
                        return n;
                    }
                    else
                    {
                        // Back out all beyond-oldBuffer code points and re-enter the buffer.
                        pos = oldBuffer.OffsetByCodePoints(length, beyond - n);
                        return beyond;
                    }
                }
                else
                {
                    // Go backwards from inside the oldBuffer.
                    pos = oldBuffer.OffsetByCodePoints(pos, -n);
                    return 0;
                }
            }

            internal void SetFirstSkipped(int c)
            {
                skipLengthAtMatch = 0;
                newBuffer.Length = 0;
                newBuffer.AppendCodePoint(c);
            }

            internal void Skip(int c)
            {
                newBuffer.AppendCodePoint(c);
            }

            internal void RecordMatch() { skipLengthAtMatch = newBuffer.Length; }

            // Replaces the characters we consumed with the newly skipped ones.
            internal void ReplaceMatch()
            {
                // Note: UnicodeString.replace() pins pos to at most length().
                int oldLength = oldBuffer.Length;
                if (pos > oldLength) { pos = oldLength; }
                oldBuffer.Delete(0, pos).Insert(0, newBuffer, 0, Math.Max(Math.Min(skipLengthAtMatch, newBuffer.Length), 0)); // ICU4N: Corrected 4th parameter
                pos = 0;
            }

            internal void SaveTrieState(CharsTrie trie) { trie.SaveState(state); }
            internal void ResetToTrieState(CharsTrie trie) { trie.ResetToState(state); }

            // Combining marks skipped in previous discontiguous-contraction matching.
            // After that discontiguous contraction was completed, we start reading them from here.
            private readonly StringBuilder oldBuffer = new StringBuilder();
            // Combining marks newly skipped in current discontiguous-contraction matching.
            // These might have been read from the normal text or from the oldBuffer.
            private readonly StringBuilder newBuffer = new StringBuilder();
            // Reading index in oldBuffer,
            // or counter for how many code points have been read beyond oldBuffer (pos-oldBuffer.length()).
            private int pos;
            // newBuffer.length() at the time of the last matching character.
            // When a partial match fails, we back out skipped and partial-matching input characters.
            private int skipLengthAtMatch;
            // We save the trie state before we attempt to match a character,
            // so that we can skip it and try the next one.
            private CharsTrie.State state = new CharsTrie.State();
        };

        /// <summary>
        /// Partially constructs the iterator.
        /// In .NET, we cache partially constructed iterators
        /// and finish their setup when starting to work on text
        /// (via <see cref="Reset(bool)"/> and the SetText(numeric, ...) methods of subclasses).
        /// <para/>
        /// In C++, there is only one constructor, and iterators are
        /// stack-allocated as needed.
        /// </summary>
        /// <param name="d"></param>
        public CollationIterator(CollationData d)
        {
            trie = d.trie;
            data = d;
            numCpFwd = -1;
            isNumeric = false;
            ceBuffer = null;
        }

        public CollationIterator(CollationData d, bool numeric)
        {
            trie = d.trie;
            data = d;
            numCpFwd = -1;
            isNumeric = numeric;
            ceBuffer = new CEBuffer();
        }

        public override bool Equals(object other)
        {
            // Subclasses: Call this method and then add more specific checks.
            // Compare the iterator state but not the collation data (trie & data fields):
            // Assume that the caller compares the data.
            // Ignore skipped since that should be unused between calls to nextCE().
            // (It only stays around to avoid another memory allocation.)
            if (other == null) { return false; }
            if (!this.GetType().Equals(other.GetType())) { return false; }
            CollationIterator o = (CollationIterator)other;
            if (!(ceBuffer.Length == o.ceBuffer.Length &&
                    cesIndex == o.cesIndex &&
                    numCpFwd == o.numCpFwd &&
                    isNumeric == o.isNumeric))
            {
                return false;
            }
            for (int i = 0; i < ceBuffer.Length; ++i)
            {
                if (ceBuffer[i] != o.ceBuffer[i]) { return false; }
            }
            return true;
        }

        public override int GetHashCode()
        {
            // Dummy return to prevent compile warnings.
            return 0;
        }

        /// <summary>
        /// Resets the iterator state and sets the position to the specified offset.
        /// Subclasses must implement, and must call the parent class method,
        /// or <see cref="CollationIterator.Reset()"/>.
        /// </summary>
        public abstract void ResetToOffset(int newOffset);

        public abstract int Offset { get; }

        /// <summary>
        /// Returns the next collation element.
        /// </summary>
        public long NextCE()
        {
            if (cesIndex < ceBuffer.Length)
            {
                // Return the next buffered CE.
                return ceBuffer[cesIndex++];
            }
            Debug.Assert(cesIndex == ceBuffer.Length);
            ceBuffer.IncLength();
            long cAndCE32 = HandleNextCE32();
            int c = (int)(cAndCE32 >> 32);
            int ce32 = (int)cAndCE32;
            int t = ce32 & 0xff;
            if (t < Collation.SPECIAL_CE32_LOW_BYTE)
            {  // Forced-inline of isSpecialCE32(ce32).
               // Normal CE from the main data.
               // Forced-inline of ceFromSimpleCE32(ce32).
                return ceBuffer[cesIndex++] =
                        ((long)(ce32 & 0xffff0000) << 32) | ((long)(ce32 & 0xff00) << 16) | (uint)(t << 8);
            }
            CollationData d;
            // The compiler should be able to optimize the previous and the following
            // comparisons of t with the same constant.
            if (t == Collation.SPECIAL_CE32_LOW_BYTE)
            {
                if (c < 0)
                {
                    return ceBuffer[cesIndex++] = Collation.NoCE;
                }
                d = data.Base;
                ce32 = d.GetCE32(c);
                t = ce32 & 0xff;
                if (t < Collation.SPECIAL_CE32_LOW_BYTE)
                {
                    // Normal CE from the base data.
                    return ceBuffer[cesIndex++] =
                            ((long)(ce32 & 0xffff0000) << 32) | ((long)(ce32 & 0xff00) << 16) | (uint)(t << 8);
                }
            }
            else
            {
                d = data;
            }
            if (t == Collation.LONG_PRIMARY_CE32_LOW_BYTE)
            {
                // Forced-inline of ceFromLongPrimaryCE32(ce32).
                return ceBuffer[cesIndex++] =
                        ((long)(ce32 - t) << 32) | Collation.CommonSecondaryAndTertiaryCE;
            }
            return NextCEFromCE32(d, c, ce32);
        }

        /// <summary>
        /// Fetches all CEs.
        /// </summary>
        /// <returns>GetCEsLength()</returns>
        public int FetchCEs()
        {
            while (NextCE() != Collation.NoCE)
            {
                // No need to loop for each expansion CE.
                cesIndex = ceBuffer.Length;
            }
            return ceBuffer.Length;
        }

        /// <summary>
        /// Overwrites the current CE (the last one returned by <see cref="NextCE()"/>).
        /// </summary>
        internal void SetCurrentCE(long ce)
        {
            Debug.Assert(cesIndex > 0);
            ceBuffer[cesIndex - 1] = ce;
        }

        /// <summary>
        /// Returns the previous collation element.
        /// </summary>
        public long PreviousCE(IList<int> offsets)
        {
            if (ceBuffer.Length > 0)
            {
                // Return the previous buffered CE.
                return ceBuffer[--ceBuffer.Length];
            }
            offsets.Clear();
            int limitOffset = Offset;
            int c = PreviousCodePoint();
            if (c < 0) { return Collation.NoCE; }
            if (data.IsUnsafeBackward(c, isNumeric))
            {
                return PreviousCEUnsafe(c, offsets);
            }
            // Simple, safe-backwards iteration:
            // Get a CE going backwards, handle prefixes but no contractions.
            int ce32 = data.GetCE32(c);
            CollationData d;
            if (ce32 == Collation.FALLBACK_CE32)
            {
                d = data.Base;
                ce32 = d.GetCE32(c);
            }
            else
            {
                d = data;
            }
            if (Collation.IsSimpleOrLongCE32(ce32))
            {
                return Collation.CeFromCE32(ce32);
            }
            AppendCEsFromCE32(d, c, ce32, false);
            if (ceBuffer.Length > 1)
            {
                offsets.Add(Offset);
                // For an expansion, the offset of each non-initial CE is the limit offset,
                // consistent with forward iteration.
                while (offsets.Count <= ceBuffer.Length)
                {
                    offsets.Add(limitOffset);
                };
            }
            return ceBuffer[--ceBuffer.Length];
        }

        public int CEsLength
        {
            get { return ceBuffer.Length; }
        }

        public long GetCE(int i)
        {
            return ceBuffer[i];
        }

        public long[] GetCEs()
        {
            return ceBuffer.GetCEs();
        }

        internal void ClearCEs()
        {
            cesIndex = ceBuffer.Length = 0;
        }

        public void ClearCEsIfNoneRemaining()
        {
            if (cesIndex == ceBuffer.Length) { ClearCEs(); }
        }

        /// <summary>
        /// Returns the next code point (with post-increment).
        /// Public for identical-level comparison and for testing.
        /// </summary>
        public abstract int NextCodePoint();

        /// <summary>
        /// Returns the previous code point (with pre-decrement).
        /// Public for identical-level comparison and for testing.
        /// </summary>
        public abstract int PreviousCodePoint();

        protected void Reset()
        {
            cesIndex = ceBuffer.Length = 0;
            if (skipped != null) { skipped.Clear(); }
        }

        /// <summary>
        /// Resets the state as well as the numeric setting,
        /// and completes the initialization.
        /// Only exists where we reset cached <see cref="CollationIterator"/> instances
        /// rather than stack-allocating temporary ones.
        /// (See also the constructor comments.)
        /// </summary>
        protected void Reset(bool numeric)
        {
            if (ceBuffer == null)
            {
                ceBuffer = new CEBuffer();
            }
            Reset();
            isNumeric = numeric;
        }

        /// <summary>
        /// Returns the next code point and its local CE32 value.
        /// Returns <see cref="Collation.FALLBACK_CE32"/> at the end of the text (c&lt;0)
        /// or when c's CE32 value is to be looked up in the base data (fallback).
        /// <para/>
        /// The code point is used for fallbacks, context and implicit weights.
        /// It is ignored when the returned CE32 is not special (e.g., FFFD_CE32).
        /// </summary>
        /// <returns>Returns the code point in bits 63..32 (signed) and the CE32 in bits 31..0.</returns>
        protected virtual long HandleNextCE32()
        {
            int c = NextCodePoint();
            if (c < 0) { return NoCodePointAndCE32; }
            return MakeCodePointAndCE32Pair(c, data.GetCE32(c));
        }
        protected virtual long MakeCodePointAndCE32Pair(int c, int ce32)
        {
            return ((long)c << 32) | (ce32 & 0xffffffffL);
        }
        protected const long NoCodePointAndCE32 = (-1L << 32) | (Collation.FALLBACK_CE32 & 0xffffffffL); // ICU4N specific - renamed from NO_CP_AND_CE32

        /// <summary>
        /// Called when <see cref="HandleNextCE32()"/> returns a LEAD_SURROGATE_TAG for a lead surrogate code unit.
        /// Returns the trail surrogate in that case and advances past it,
        /// if a trail surrogate follows the lead surrogate.
        /// Otherwise returns any other code unit and does not advance.
        /// </summary>
        protected virtual char HandleGetTrailSurrogate()
        {
            return (char)0;
        }

        /////**
        //// * Called when handleNextCE32() returns with c==0, to see whether it is a NUL terminator.
        //// * (Not needed in Java.)
        //// */
        /////*protected boolean foundNULTerminator() {
        ////    return false;
        ////}*/

        /// <summary>
        /// false if surrogate code points U+D800..U+DFFF
        ///         map to their own implicit primary weights (for UTF-16),
        ///         or true if they map to CE(U+FFFD) (for UTF-8)
        /// </summary>
        protected virtual bool ForbidSurrogateCodePoints
        {
            get { return false; }
        }

        protected abstract void ForwardNumCodePoints(int num);

        protected abstract void BackwardNumCodePoints(int num);

        /// <summary>
        /// Returns the CE32 from the data trie.
        /// Normally the same as <c>data.GetCE32()</c>, but overridden in the builder.
        /// Call this only when the faster <c>data.GetCE32()</c> cannot be used.
        /// </summary>
        protected virtual int GetDataCE32(int c)
        {
            return data.GetCE32(c);
        }

        protected virtual int GetCE32FromBuilderData(int ce32)
        {
            throw new ICUException("internal program error: should be unreachable");
        }

        protected void AppendCEsFromCE32(CollationData d, int c, int ce32,
                               bool forward)
        {
            while (Collation.IsSpecialCE32(ce32))
            {
                switch (Collation.TagFromCE32(ce32))
                {
                    case Collation.FALLBACK_TAG:
                    case Collation.RESERVED_TAG_3:
                        throw new ICUException("internal program error: should be unreachable");
                    case Collation.LONG_PRIMARY_TAG:
                        ceBuffer.Append(Collation.CeFromLongPrimaryCE32(ce32));
                        return;
                    case Collation.LONG_SECONDARY_TAG:
                        ceBuffer.Append(Collation.CeFromLongSecondaryCE32(ce32));
                        return;
                    case Collation.LATIN_EXPANSION_TAG:
                        ceBuffer.EnsureAppendCapacity(2);
                        ceBuffer[ceBuffer.Length] = Collation.LatinCE0FromCE32(ce32);
                        ceBuffer[ceBuffer.Length + 1] = Collation.LatinCE1FromCE32(ce32);
                        ceBuffer.Length += 2;
                        return;
                    case Collation.EXPANSION32_TAG:
                        {
                            int index = Collation.IndexFromCE32(ce32);
                            int length = Collation.LengthFromCE32(ce32);
                            ceBuffer.EnsureAppendCapacity(length);
                            do
                            {
                                ceBuffer.AppendUnsafe(Collation.CeFromCE32(d.ce32s[index++]));
                            } while (--length > 0);
                            return;
                        }
                    case Collation.EXPANSION_TAG:
                        {
                            int index = Collation.IndexFromCE32(ce32);
                            int length = Collation.LengthFromCE32(ce32);
                            ceBuffer.EnsureAppendCapacity(length);
                            do
                            {
                                ceBuffer.AppendUnsafe(d.ces[index++]);
                            } while (--length > 0);
                            return;
                        }
                    case Collation.BUILDER_DATA_TAG:
                        ce32 = GetCE32FromBuilderData(ce32);
                        if (ce32 == Collation.FALLBACK_CE32)
                        {
                            d = data.Base;
                            ce32 = d.GetCE32(c);
                        }
                        break;
                    case Collation.PREFIX_TAG:
                        if (forward) { BackwardNumCodePoints(1); }
                        ce32 = GetCE32FromPrefix(d, ce32);
                        if (forward) { ForwardNumCodePoints(1); }
                        break;
                    case Collation.CONTRACTION_TAG:
                        {
                            int index = Collation.IndexFromCE32(ce32);
                            int defaultCE32 = d.GetCE32FromContexts(index);  // Default if no suffix match.
                            if (!forward)
                            {
                                // Backward contractions are handled by previousCEUnsafe().
                                // c has contractions but they were not found.
                                ce32 = defaultCE32;
                                break;
                            }
                            int nextCp;
                            if (skipped == null && numCpFwd < 0)
                            {
                                // Some portion of nextCE32FromContraction() pulled out here as an ASCII fast path,
                                // avoiding the function call and the nextSkippedCodePoint() overhead.
                                nextCp = NextCodePoint();
                                if (nextCp < 0)
                                {
                                    // No more text.
                                    ce32 = defaultCE32;
                                    break;
                                }
                                else if ((ce32 & Collation.CONTRACT_NEXT_CCC) != 0 &&
                                      !CollationFCD.MayHaveLccc(nextCp))
                                {
                                    // All contraction suffixes start with characters with lccc!=0
                                    // but the next code point has lccc==0.
                                    BackwardNumCodePoints(1);
                                    ce32 = defaultCE32;
                                    break;
                                }
                            }
                            else
                            {
                                nextCp = NextSkippedCodePoint();
                                if (nextCp < 0)
                                {
                                    // No more text.
                                    ce32 = defaultCE32;
                                    break;
                                }
                                else if ((ce32 & Collation.CONTRACT_NEXT_CCC) != 0 &&
                                      !CollationFCD.MayHaveLccc(nextCp))
                                {
                                    // All contraction suffixes start with characters with lccc!=0
                                    // but the next code point has lccc==0.
                                    BackwardNumSkipped(1);
                                    ce32 = defaultCE32;
                                    break;
                                }
                            }
                            ce32 = NextCE32FromContraction(d, ce32, d.contexts, index + 2, defaultCE32, nextCp);
                            if (ce32 == Collation.NO_CE32)
                            {
                                // CEs from a discontiguous contraction plus the skipped combining marks
                                // have been appended already.
                                return;
                            }
                            break;
                        }
                    case Collation.DIGIT_TAG:
                        if (isNumeric)
                        {
                            AppendNumericCEs(ce32, forward);
                            return;
                        }
                        else
                        {
                            // Fetch the non-numeric-collation CE32 and continue.
                            ce32 = d.ce32s[Collation.IndexFromCE32(ce32)];
                            break;
                        }
                    case Collation.U0000_TAG:
                        Debug.Assert(c == 0);
                        // NUL-terminated input not supported in Java.
                        // Fetch the normal ce32 for U+0000 and continue.
                        ce32 = d.ce32s[0];
                        break;
                    case Collation.HANGUL_TAG:
                        {
                            int[] jamoCE32s = d.jamoCE32s;
                            c -= Hangul.HANGUL_BASE;
                            int t = c % Hangul.JAMO_T_COUNT;
                            c /= Hangul.JAMO_T_COUNT;
                            int v = c % Hangul.JAMO_V_COUNT;
                            c /= Hangul.JAMO_V_COUNT;
                            if ((ce32 & Collation.HANGUL_NO_SPECIAL_JAMO) != 0)
                            {
                                // None of the Jamo CE32s are isSpecialCE32().
                                // Avoid recursive function calls and per-Jamo tests.
                                ceBuffer.EnsureAppendCapacity(t == 0 ? 2 : 3);
                                ceBuffer[ceBuffer.Length] = Collation.CeFromCE32(jamoCE32s[c]);
                                ceBuffer[ceBuffer.Length + 1] = Collation.CeFromCE32(jamoCE32s[19 + v]);
                                ceBuffer.Length += 2;
                                if (t != 0)
                                {
                                    ceBuffer.AppendUnsafe(Collation.CeFromCE32(jamoCE32s[39 + t]));
                                }
                                return;
                            }
                            else
                            {
                                // We should not need to compute each Jamo code point.
                                // In particular, there should be no offset or implicit ce32.
                                AppendCEsFromCE32(d, Collation.SentinelCodePoint, jamoCE32s[c], forward);
                                AppendCEsFromCE32(d, Collation.SentinelCodePoint, jamoCE32s[19 + v], forward);
                                if (t == 0) { return; }
                                // offset 39 = 19 + 21 - 1:
                                // 19 = JAMO_L_COUNT
                                // 21 = JAMO_T_COUNT
                                // -1 = omit t==0
                                ce32 = jamoCE32s[39 + t];
                                c = Collation.SentinelCodePoint;
                                break;
                            }
                        }
                    case Collation.LEAD_SURROGATE_TAG:
                        {
                            Debug.Assert(forward);  // Backward iteration should never see lead surrogate code _unit_ data.
                            Debug.Assert(IsLeadSurrogate(c));
                            char trail;
                            if (char.IsLowSurrogate(trail = HandleGetTrailSurrogate()))
                            {
                                c = Character.ToCodePoint((char)c, trail);
                                ce32 &= Collation.LEAD_TYPE_MASK;
                                if (ce32 == Collation.LEAD_ALL_UNASSIGNED)
                                {
                                    ce32 = Collation.UNASSIGNED_CE32;  // unassigned-implicit
                                }
                                else if (ce32 == Collation.LEAD_ALL_FALLBACK ||
                                      (ce32 = d.GetCE32FromSupplementary(c)) == Collation.FALLBACK_CE32)
                                {
                                    // fall back to the base data
                                    d = d.Base;
                                    ce32 = d.GetCE32FromSupplementary(c);
                                }
                            }
                            else
                            {
                                // c is an unpaired surrogate.
                                ce32 = Collation.UNASSIGNED_CE32;
                            }
                            break;
                        }
                    case Collation.OFFSET_TAG:
                        Debug.Assert(c >= 0);
                        ceBuffer.Append(d.GetCEFromOffsetCE32(c, ce32));
                        return;
                    case Collation.IMPLICIT_TAG:
                        Debug.Assert(c >= 0);
                        if (IsSurrogate(c) && ForbidSurrogateCodePoints)
                        {
                            ce32 = Collation.FFFD_CE32;
                            break;
                        }
                        else
                        {
                            ceBuffer.Append(Collation.UnassignedCEFromCodePoint(c));
                            return;
                        }
                }
            }
            ceBuffer.Append(Collation.CeFromSimpleCE32(ce32));
        }

        // TODO: Propose widening the UTF16 method.
        private static bool IsSurrogate(int c)
        {
            return (c & 0xfffff800) == 0xd800;
        }

        // TODO: Propose widening the UTF16 method.
        protected static bool IsLeadSurrogate(int c)
        {
            return (c & 0xfffffc00) == 0xd800;
        }

        // TODO: Propose widening the UTF16 method.
        protected static bool IsTrailSurrogate(int c)
        {
            return (c & 0xfffffc00) == 0xdc00;
        }

        // Main lookup trie of the data object.
        protected readonly Trie2_32 trie;
        protected readonly CollationData data;

        private long NextCEFromCE32(CollationData d, int c, int ce32)
        {
            --ceBuffer.Length;  // Undo ceBuffer.incLength().
            AppendCEsFromCE32(d, c, ce32, true);
            return ceBuffer[cesIndex++];
        }

        private int GetCE32FromPrefix(CollationData d, int ce32)
        {
            int index = Collation.IndexFromCE32(ce32);
            ce32 = d.GetCE32FromContexts(index);  // Default if no prefix match.
            index += 2;
            // Number of code points read before the original code point.
            int lookBehind = 0;
            CharsTrie prefixes = new CharsTrie(d.contexts, index);
            for (; ; )
            {
                int c = PreviousCodePoint();
                if (c < 0) { break; }
                ++lookBehind;
                Result match = prefixes.NextForCodePoint(c);
                if (match.HasValue())
                {
                    ce32 = prefixes.GetValue();
                }
                if (!match.HasNext()) { break; }
            }
            ForwardNumCodePoints(lookBehind);
            return ce32;
        }

        private int NextSkippedCodePoint()
        {
            if (skipped != null && skipped.HasNext) { return skipped.Next(); }
            if (numCpFwd == 0) { return Collation.SentinelCodePoint; }
            int c = NextCodePoint();
            if (skipped != null && !skipped.IsEmpty && c >= 0) { skipped.IncBeyond(); }
            if (numCpFwd > 0 && c >= 0) { --numCpFwd; }
            return c;
        }

        private void BackwardNumSkipped(int n)
        {
            if (skipped != null && !skipped.IsEmpty)
            {
                n = skipped.BackwardNumCodePoints(n);
            }
            BackwardNumCodePoints(n);
            if (numCpFwd >= 0) { numCpFwd += n; }
        }

        private int NextCE32FromContraction(
                CollationData d, int contractionCE32,
                string trieChars, int trieOffset, int ce32, int c) // ICU4N specific - changed trieChars from ICharSequence to string
        {
            // c: next code point after the original one

            // Number of code points read beyond the original code point.
            // Needed for discontiguous contraction matching.
            int lookAhead = 1;
            // Number of code points read since the last match (initially only c).
            int sinceMatch = 1;
            // Normally we only need a contiguous match,
            // and therefore need not remember the suffixes state from before a mismatch for retrying.
            // If we are already processing skipped combining marks, then we do track the state.
            CharsTrie suffixes = new CharsTrie(trieChars, trieOffset);
            if (skipped != null && !skipped.IsEmpty) { skipped.SaveTrieState(suffixes); }
            Result match = suffixes.FirstForCodePoint(c);
            for (; ; )
            {
                int nextCp;
                if (match.HasValue())
                {
                    ce32 = suffixes.GetValue();
                    if (!match.HasNext() || (c = NextSkippedCodePoint()) < 0)
                    {
                        return ce32;
                    }
                    if (skipped != null && !skipped.IsEmpty) { skipped.SaveTrieState(suffixes); }
                    sinceMatch = 1;
                }
                else if (match == Result.NoMatch || (nextCp = NextSkippedCodePoint()) < 0)
                {
                    // No match for c, or partial match (BytesTrie.Result.NO_VALUE) and no further text.
                    // Back up if necessary, and try a discontiguous contraction.
                    if ((contractionCE32 & Collation.CONTRACT_TRAILING_CCC) != 0 &&
                            // Discontiguous contraction matching extends an existing match.
                            // If there is no match yet, then there is nothing to do.
                            ((contractionCE32 & Collation.CONTRACT_SINGLE_CP_NO_MATCH) == 0 ||
                                sinceMatch < lookAhead))
                    {
                        // The last character of at least one suffix has lccc!=0,
                        // allowing for discontiguous contractions.
                        // UCA S2.1.1 only processes non-starters immediately following
                        // "a match in the table" (sinceMatch=1).
                        if (sinceMatch > 1)
                        {
                            // Return to the state after the last match.
                            // (Return to sinceMatch=0 and re-fetch the first partially-matched character.)
                            BackwardNumSkipped(sinceMatch);
                            c = NextSkippedCodePoint();
                            lookAhead -= sinceMatch - 1;
                            sinceMatch = 1;
                        }
                        if (d.GetFCD16(c) > 0xff)
                        {
                            return NextCE32FromDiscontiguousContraction(
                                d, suffixes, ce32, lookAhead, c);
                        }
                    }
                    break;
                }
                else
                {
                    // Continue after partial match (BytesTrie.Result.NO_VALUE) for c.
                    // It does not have a result value, therefore it is not itself "a match in the table".
                    // If a partially-matched c has ccc!=0 then
                    // it might be skipped in discontiguous contraction.
                    c = nextCp;
                    ++sinceMatch;
                }
                ++lookAhead;
                match = suffixes.NextForCodePoint(c);
            }
            BackwardNumSkipped(sinceMatch);
            return ce32;
        }

        private int NextCE32FromDiscontiguousContraction(
                CollationData d, CharsTrie suffixes, int ce32,
                int lookAhead, int c)
        {
            // UCA section 3.3.2 Contractions:
            // Contractions that end with non-starter characters
            // are known as discontiguous contractions.
            // ... discontiguous contractions must be detected in input text
            // whenever the final sequence of non-starter characters could be rearranged
            // so as to make a contiguous matching sequence that is canonically equivalent.

            // UCA: http://www.unicode.org/reports/tr10/#S2.1
            // S2.1 Find the longest initial substring S at each point that has a match in the table.
            // S2.1.1 If there are any non-starters following S, process each non-starter C.
            // S2.1.2 If C is not blocked from S, find if S + C has a match in the table.
            //     Note: A non-starter in a string is called blocked
            //     if there is another non-starter of the same canonical combining class or zero
            //     between it and the last character of canonical combining class 0.
            // S2.1.3 If there is a match, replace S by S + C, and remove C.

            // First: Is a discontiguous contraction even possible?
            int fcd16 = d.GetFCD16(c);
            Debug.Assert(fcd16 > 0xff);  // The caller checked this already, as a shortcut.
            int nextCp = NextSkippedCodePoint();
            if (nextCp < 0)
            {
                // No further text.
                BackwardNumSkipped(1);
                return ce32;
            }
            ++lookAhead;
            int prevCC = fcd16 & 0xff;
            fcd16 = d.GetFCD16(nextCp);
            if (fcd16 <= 0xff)
            {
                // The next code point after c is a starter (S2.1.1 "process each non-starter").
                BackwardNumSkipped(2);
                return ce32;
            }

            // We have read and matched (lookAhead-2) code points,
            // read non-matching c and peeked ahead at nextCp.
            // Return to the state before the mismatch and continue matching with nextCp.
            if (skipped == null || skipped.IsEmpty)
            {
                if (skipped == null)
                {
                    skipped = new SkippedState();
                }
                suffixes.Reset();
                if (lookAhead > 2)
                {
                    // Replay the partial match so far.
                    BackwardNumCodePoints(lookAhead);
                    suffixes.FirstForCodePoint(NextCodePoint());
                    for (int i = 3; i < lookAhead; ++i)
                    {
                        suffixes.NextForCodePoint(NextCodePoint());
                    }
                    // Skip c (which did not match) and nextCp (which we will try now).
                    ForwardNumCodePoints(2);
                }
                skipped.SaveTrieState(suffixes);
            }
            else
            {
                // Reset to the trie state before the failed match of c.
                skipped.ResetToTrieState(suffixes);
            }

            skipped.SetFirstSkipped(c);
            // Number of code points read since the last match (at this point: c and nextCp).
            int sinceMatch = 2;
            c = nextCp;
            for (; ; )
            {
                Result match;
                // "If C is not blocked from S, find if S + C has a match in the table." (S2.1.2)
                if (prevCC < (fcd16 >> 8) && (match = suffixes.NextForCodePoint(c)).HasValue())
                {
                    // "If there is a match, replace S by S + C, and remove C." (S2.1.3)
                    // Keep prevCC unchanged.
                    ce32 = suffixes.GetValue();
                    sinceMatch = 0;
                    skipped.RecordMatch();
                    if (!match.HasNext()) { break; }
                    skipped.SaveTrieState(suffixes);
                }
                else
                {
                    // No match for "S + C", skip C.
                    skipped.Skip(c);
                    skipped.ResetToTrieState(suffixes);
                    prevCC = fcd16 & 0xff;
                }
                if ((c = NextSkippedCodePoint()) < 0) { break; }
                ++sinceMatch;
                fcd16 = d.GetFCD16(c);
                if (fcd16 <= 0xff)
                {
                    // The next code point after c is a starter (S2.1.1 "process each non-starter").
                    break;
                }
            }
            BackwardNumSkipped(sinceMatch);
            bool isTopDiscontiguous = skipped.IsEmpty;
            skipped.ReplaceMatch();
            if (isTopDiscontiguous && !skipped.IsEmpty)
            {
                // We did get a match after skipping one or more combining marks,
                // and we are not in a recursive discontiguous contraction.
                // Append CEs from the contraction ce32
                // and then from the combining marks that we skipped before the match.
                c = Collation.SentinelCodePoint;
                for (; ; )
                {
                    AppendCEsFromCE32(d, c, ce32, true);
                    // Fetch CE32s for skipped combining marks from the normal data, with fallback,
                    // rather than from the CollationData where we found the contraction.
                    if (!skipped.HasNext) { break; }
                    c = skipped.Next();
                    ce32 = GetDataCE32(c);
                    if (ce32 == Collation.FALLBACK_CE32)
                    {
                        d = data.Base;
                        ce32 = d.GetCE32(c);
                    }
                    else
                    {
                        d = data;
                    }
                    // Note: A nested discontiguous-contraction match
                    // replaces consumed combining marks with newly skipped ones
                    // and resets the reading position to the beginning.
                }
                skipped.Clear();
                ce32 = Collation.NO_CE32;  // Signal to the caller that the result is in the ceBuffer.
            }
            return ce32;
        }

        /// <summary>
        /// Returns the previous CE when <c>data.IsUnsafeBackward(c, isNumeric)</c>.
        /// </summary>
        private long PreviousCEUnsafe(int c, IList<int> offsets)
        {
            // We just move through the input counting safe and unsafe code points
            // without collecting the unsafe-backward substring into a buffer and
            // switching to it.
            // This is to keep the logic simple. Otherwise we would have to handle
            // prefix matching going before the backward buffer, switching
            // to iteration and back, etc.
            // In the most important case of iterating over a normal string,
            // reading from the string itself is already maximally fast.
            // The only drawback there is that after getting the CEs we always
            // skip backward to the safe character rather than switching out
            // of a backwardBuffer.
            // But this should not be the common case for previousCE(),
            // and correctness and maintainability are more important than
            // complex optimizations.
            // Find the first safe character before c.
            int numBackward = 1;
            while ((c = PreviousCodePoint()) >= 0)
            {
                ++numBackward;
                if (!data.IsUnsafeBackward(c, isNumeric))
                {
                    break;
                }
            }
            // Set the forward iteration limit.
            // Note: This counts code points.
            // We cannot enforce a limit in the middle of a surrogate pair or similar.
            numCpFwd = numBackward;
            // Reset the forward iterator.
            cesIndex = 0;
            Debug.Assert(ceBuffer.Length == 0);
            // Go forward and collect the CEs.
            int offset = Offset;
            while (numCpFwd > 0)
            {
                // nextCE() normally reads one code point.
                // Contraction matching and digit specials read more and check numCpFwd.
                --numCpFwd;
                // Append one or more CEs to the ceBuffer.
                NextCE();
                Debug.Assert(ceBuffer[ceBuffer.Length - 1] != Collation.NoCE);
                // No need to loop for getting each expansion CE from nextCE().
                cesIndex = ceBuffer.Length;
                // However, we need to write an offset for each CE.
                // This is for CollationElementIterator.getOffset() to return
                // intermediate offsets from the unsafe-backwards segment.
                Debug.Assert(offsets.Count < ceBuffer.Length);
                offsets.Add(offset);
                // For an expansion, the offset of each non-initial CE is the limit offset,
                // consistent with forward iteration.
                offset = Offset;
                while (offsets.Count < ceBuffer.Length)
                {
                    offsets.Add(offset);
                };
            }
            Debug.Assert(offsets.Count == ceBuffer.Length);
            // End offset corresponding to just after the unsafe-backwards segment.
            offsets.Add(offset);
            // Reset the forward iteration limit
            // and move backward to before the segment for which we fetched CEs.
            numCpFwd = -1;
            BackwardNumCodePoints(numBackward);
            // Use the collected CEs and return the last one.
            cesIndex = 0;  // Avoid cesIndex > ceBuffer.length when that gets decremented.
            return ceBuffer[--ceBuffer.Length];
        }

        /// <summary>
        /// Turns a string of digits (bytes 0..9)
        /// into a sequence of CEs that will sort in numeric order.
        /// <para/>
        /// Starts from this <paramref name="ce32"/>'s digit value and consumes the following/preceding digits.
        /// The digits string must not be empty and must not have leading zeros.
        /// </summary>
        private void AppendNumericCEs(int ce32, bool forward)
        {
            // Collect digits.
            // TODO: Use some kind of a byte buffer? We only store values 0..9.
            StringBuilder digits = new StringBuilder();
            if (forward)
            {
                for (; ; )
                {
                    char digit = Collation.DigitFromCE32(ce32);
                    digits.Append(digit);
                    if (numCpFwd == 0) { break; }
                    int c = NextCodePoint();
                    if (c < 0) { break; }
                    ce32 = data.GetCE32(c);
                    if (ce32 == Collation.FALLBACK_CE32)
                    {
                        ce32 = data.Base.GetCE32(c);
                    }
                    if (!Collation.HasCE32Tag(ce32, Collation.DIGIT_TAG))
                    {
                        BackwardNumCodePoints(1);
                        break;
                    }
                    if (numCpFwd > 0) { --numCpFwd; }
                }
            }
            else
            {
                for (; ; )
                {
                    char digit = Collation.DigitFromCE32(ce32);
                    digits.Append(digit);
                    int c = PreviousCodePoint();
                    if (c < 0) { break; }
                    ce32 = data.GetCE32(c);
                    if (ce32 == Collation.FALLBACK_CE32)
                    {
                        ce32 = data.Base.GetCE32(c);
                    }
                    if (!Collation.HasCE32Tag(ce32, Collation.DIGIT_TAG))
                    {
                        ForwardNumCodePoints(1);
                        break;
                    }
                }
                // Reverse the digit string.
                digits.Reverse();
            }
            int pos = 0;
            do
            {
                // Skip leading zeros.
                while (pos < (digits.Length - 1) && digits[pos] == 0) { ++pos; }
                // Write a sequence of CEs for at most 254 digits at a time.
                int segmentLength = digits.Length - pos;
                if (segmentLength > 254) { segmentLength = 254; }
                AppendNumericSegmentCEs(digits.SubSequence(pos, pos + segmentLength));
                pos += segmentLength;
            } while (pos < digits.Length);
        }

        /// <summary>
        /// Turns 1..254 digits into a sequence of CEs.
        /// Called by <see cref="AppendNumericCEs(int, bool)"/> for each segment of at most 254 digits.
        /// </summary>
        private void AppendNumericSegmentCEs(ICharSequence digits)
        {
            int length = digits.Length;
            Debug.Assert(1 <= length && length <= 254);
            Debug.Assert(length == 1 || digits[0] != 0);
            long numericPrimary = data.numericPrimary;
            // Note: We use primary byte values 2..255: digits are not compressible.
            if (length <= 7)
            {
                // Very dense encoding for small numbers.
                int value = digits[0];
                for (int i = 1; i < length; ++i)
                {
                    value = value * 10 + digits[i];
                }
                // Primary weight second byte values:
                //     74 byte values   2.. 75 for small numbers in two-byte primary weights.
                //     40 byte values  76..115 for medium numbers in three-byte primary weights.
                //     16 byte values 116..131 for large numbers in four-byte primary weights.
                //    124 byte values 132..255 for very large numbers with 4..127 digit pairs.
                int firstByte = 2;
                int numBytes = 74;
                if (value < numBytes)
                {
                    // Two-byte primary for 0..73, good for day & month numbers etc.
                    long primary2 = numericPrimary | (uint)((firstByte + value) << 16);
                    ceBuffer.Append(Collation.MakeCE(primary2));
                    return;
                }
                value -= numBytes;
                firstByte += numBytes;
                numBytes = 40;
                if (value < numBytes * 254)
                {
                    // Three-byte primary for 74..10233=74+40*254-1, good for year numbers and more.
                    long primary3 = numericPrimary |
                        (uint)((firstByte + value / 254) << 16) | (uint)((2 + value % 254) << 8);
                    ceBuffer.Append(Collation.MakeCE(primary3));
                    return;
                }
                value -= numBytes * 254;
                firstByte += numBytes;
                numBytes = 16;
                if (value < numBytes * 254 * 254)
                {
                    // Four-byte primary for 10234..1042489=10234+16*254*254-1.
                    long primary4 = numericPrimary | (uint)(2 + value % 254);
                    value /= 254;
                    primary4 |= (uint)(2 + value % 254) << 8;
                    value /= 254;
                    primary4 |= (uint)(firstByte + value % 254) << 16;
                    ceBuffer.Append(Collation.MakeCE(primary4));
                    return;
                }
                // original value > 1042489
            }
            Debug.Assert(length >= 7);

            // The second primary byte value 132..255 indicates the number of digit pairs (4..127),
            // then we generate primary bytes with those pairs.
            // Omit trailing 00 pairs.
            // Decrement the value for the last pair.

            // Set the exponent. 4 pairs.132, 5 pairs.133, ..., 127 pairs.255.
            int numPairs = (length + 1) / 2;
            long primary = numericPrimary | (uint)((132 - 4 + numPairs) << 16);
            // Find the length without trailing 00 pairs.
            while (digits[length - 1] == 0 && digits[length - 2] == 0)
            {
                length -= 2;
            }
            // Read the first pair.
            int pair;
            int pos;
            if ((length & 1) != 0)
            {
                // Only "half a pair" if we have an odd number of digits.
                pair = digits[0];
                pos = 1;
            }
            else
            {
                pair = digits[0] * 10 + digits[1];
                pos = 2;
            }
            pair = 11 + 2 * pair;
            // Add the pairs of digits between pos and length.
            int shift = 8;
            while (pos < length)
            {
                if (shift == 0)
                {
                    // Every three pairs/bytes we need to store a 4-byte-primary CE
                    // and start with a new CE with the '0' primary lead byte.
                    primary |= (uint)pair;
                    ceBuffer.Append(Collation.MakeCE(primary));
                    primary = numericPrimary;
                    shift = 16;
                }
                else
                {
                    primary |= (uint)pair << shift;
                    shift -= 8;
                }
                pair = 11 + 2 * (digits[pos] * 10 + digits[pos + 1]);
                pos += 2;
            }
            primary |= (uint)(pair - 1) << shift;
            ceBuffer.Append(Collation.MakeCE(primary));
        }

        private CEBuffer ceBuffer;
        private int cesIndex;

        private SkippedState skipped;

        // Number of code points to read forward, or -1.
        // Used as a forward iteration limit in previousCEUnsafe().
        private int numCpFwd;
        // Numeric collation (CollationSettings.NUMERIC).
        private bool isNumeric;
    }
}
