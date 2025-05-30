﻿using ICU4N.Support.Text;
using ICU4N.Text;
using ICU4N.Util;
using J2N;
using J2N.IO;
using J2N.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
#nullable enable

namespace ICU4N.Impl
{
    public static partial class Hangul
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

        #region AppendHangulDecomposition(int, IAppendable)
        /// <summary>
        /// Decomposes <paramref name="c"/>, which must be a Hangul syllable, into buffer
        /// and returns the length of the decomposition (2 or 3).
        /// </summary>
        public static int AppendHangulDecomposition(this StringBuilder buffer, int c)
        {
            // ICU4N: Added guard clause
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            // ICU4N: Removed unnecessary try/catch for IOException
            c -= HangulBase;
            int c2 = c % JamoTCount;
            c /= JamoTCount;
            buffer.Append((char)(JamoLBase + c / JamoVCount));
            buffer.Append((char)(JamoVBase + c % JamoVCount));
            if (c2 == 0)
            {
                return 2;
            }
            else
            {
                buffer.Append((char)(JamoTBase + c2));
                return 3;
            }
        }



        /// <summary>
        /// Decomposes <paramref name="c"/>, which must be a Hangul syllable, into buffer
        /// and returns the length of the decomposition (2 or 3).
        /// </summary>
        public static int AppendHangulDecomposition(this IAppendable buffer, int c)
        {
            // ICU4N: Added guard clause
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            // ICU4N: Removed unnecessary try/catch for IOException
            c -= HangulBase;
            int c2 = c % JamoTCount;
            c /= JamoTCount;
            buffer.Append((char)(JamoLBase + c / JamoVCount));
            buffer.Append((char)(JamoVBase + c % JamoVCount));
            if (c2 == 0)
            {
                return 2;
            }
            else
            {
                buffer.Append((char)(JamoTBase + c2));
                return 3;
            }
        }


        /// <summary>
        /// Decomposes <paramref name="c"/>, which must be a Hangul syllable, into buffer
        /// and returns the length of the decomposition (2 or 3).
        /// </summary>
        internal static int AppendHangulDecomposition(this ref ValueStringBuilder buffer, int c)
        {
            // ICU4N: Removed unnecessary try/catch for IOException
            c -= HangulBase;
            int c2 = c % JamoTCount;
            c /= JamoTCount;
            buffer.Append((char)(JamoLBase + c / JamoVCount));
            buffer.Append((char)(JamoVBase + c % JamoVCount));
            if (c2 == 0)
            {
                return 2;
            }
            else
            {
                buffer.Append((char)(JamoTBase + c2));
                return 3;
            }
        }


        /// <summary>
        /// Decomposes <paramref name="c"/>, which must be a Hangul syllable, into buffer
        /// and returns the length of the decomposition (2 or 3).
        /// </summary>
        public static int AppendHangulDecomposition(this ref ReorderingBuffer buffer, int c)
        {
            // ICU4N: Removed unnecessary try/catch for IOException
            c -= HangulBase;
            int c2 = c % JamoTCount;
            c /= JamoTCount;
            buffer.Append((char)(JamoLBase + c / JamoVCount));
            buffer.Append((char)(JamoVBase + c % JamoVCount));
            if (c2 == 0)
            {
                return 2;
            }
            else
            {
                buffer.Append((char)(JamoTBase + c2));
                return 3;
            }
        }

        #endregion AppendHangulDecomposition(int, IAppendable)

        /// <summary>
        /// Decomposes <paramref name="c"/>, which must be a Hangul syllable, into buffer
        /// and returns the length of the decomposition (2 or 3).
        /// </summary>
        /// <param name="c">The codepoint to decompose.</param>
        /// <param name="buffer">A buffer with length >= 3 to write the output.</param>
        /// <returns>The decomposed text, sliced to the correct length.</returns>
        public static ReadOnlySpan<char> GetDecomposition(int c, Span<char> buffer) // Buffer should be length 3
        {
            if (buffer.Length < 3)
                throw new ArgumentException("Buffer must be at least 3 characters in length.");
            int charsLength;
            c -= HangulBase;
            int c2 = c % JamoTCount;
            c /= JamoTCount;
            buffer[0] = (char)(JamoLBase + c / JamoVCount);
            buffer[1] = (char)(JamoVBase + c % JamoVCount);
            if (c2 == 0)
            {
                charsLength = 2;
            }
            else
            {
                buffer[2] = (char)(JamoTBase + c2);
                charsLength = 3;
            }
            return buffer.Slice(0, charsLength);
        }

        #region AppendHangulRawDecomposition(int, IAppendable)
        /// <summary>
        /// Decomposes <paramref name="c"/>, which must be a Hangul syllable, into buffer.
        /// This is the raw, not recursive, decomposition. Its length is always 2.
        /// </summary>
        public static void AppendHangulRawDecomposition(this StringBuilder buffer, int c)
        {
            // ICU4N: Added guard clause
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            // ICU4N: Removed unnecessary try/catch for IOException
            int orig = c;
            c -= HangulBase;
            int c2 = c % JamoTCount;
            if (c2 == 0)
            {
                c /= JamoTCount;
                buffer.Append((char)(JamoLBase + c / JamoVCount));
                buffer.Append((char)(JamoVBase + c % JamoVCount));
            }
            else
            {
                buffer.Append((char)(orig - c2));  // LV syllable
                buffer.Append((char)(JamoTBase + c2));
            }
        }

        /// <summary>
        /// Decomposes <paramref name="c"/>, which must be a Hangul syllable, into buffer.
        /// This is the raw, not recursive, decomposition. Its length is always 2.
        /// </summary>
        public static void AppendHangulRawDecomposition(this IAppendable buffer, int c)
        {
            // ICU4N: Added guard clause
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            // ICU4N: Removed unnecessary try/catch for IOException
            int orig = c;
            c -= HangulBase;
            int c2 = c % JamoTCount;
            if (c2 == 0)
            {
                c /= JamoTCount;
                buffer.Append((char)(JamoLBase + c / JamoVCount));
                buffer.Append((char)(JamoVBase + c % JamoVCount));
            }
            else
            {
                buffer.Append((char)(orig - c2));  // LV syllable
                buffer.Append((char)(JamoTBase + c2));
            }
        }

        /// <summary>
        /// Decomposes <paramref name="c"/>, which must be a Hangul syllable, into buffer.
        /// This is the raw, not recursive, decomposition. Its length is always 2.
        /// </summary>
        internal static void AppendHangulRawDecomposition(this ref ValueStringBuilder buffer, int c)
        {
            // ICU4N: Removed unnecessary try/catch for IOException
            int orig = c;
            c -= HangulBase;
            int c2 = c % JamoTCount;
            if (c2 == 0)
            {
                c /= JamoTCount;
                buffer.Append((char)(JamoLBase + c / JamoVCount));
                buffer.Append((char)(JamoVBase + c % JamoVCount));
            }
            else
            {
                buffer.Append((char)(orig - c2));  // LV syllable
                buffer.Append((char)(JamoTBase + c2));
            }
        }

        /// <summary>
        /// Decomposes <paramref name="c"/>, which must be a Hangul syllable, into buffer.
        /// This is the raw, not recursive, decomposition. Its length is always 2.
        /// </summary>
        public static void AppendHangulRawDecomposition(this ref ReorderingBuffer buffer, int c)
        {
            // ICU4N: Removed unnecessary try/catch for IOException
            int orig = c;
            c -= HangulBase;
            int c2 = c % JamoTCount;
            if (c2 == 0)
            {
                c /= JamoTCount;
                buffer.Append((char)(JamoLBase + c / JamoVCount));
                buffer.Append((char)(JamoVBase + c % JamoVCount));
            }
            else
            {
                buffer.Append((char)(orig - c2));  // LV syllable
                buffer.Append((char)(JamoTBase + c2));
            }
        }

        #endregion AppendHangulRawDecomposition(int, IAppendable)

        /// <summary>
        /// Decomposes <paramref name="c"/>, which must be a Hangul syllable, into buffer.
        /// This is the raw, not recursive, decomposition. Its length is always 2.
        /// </summary>
        /// <param name="c">The codepoint to decompose.</param>
        /// <param name="buffer">A buffer with length >= 2 to write the output.</param>
        /// <returns>The decomposed text, sliced to the correct length.</returns>
        public static ReadOnlySpan<char> GetRawDecomposition(int c, Span<char> buffer) // Buffer should be length 2
        {
            if (buffer.Length < 2)
                throw new ArgumentException("Buffer must be at least 2 characters in length.");
            // ICU4N: Removed unnecessary try/catch for IOException
            int orig = c;
            c -= HangulBase;
            int c2 = c % JamoTCount;
            if (c2 == 0)
            {
                c /= JamoTCount;
                buffer[0] = (char)(JamoLBase + c / JamoVCount);
                buffer[1] = (char)(JamoVBase + c % JamoVCount);
            }
            else
            {
                buffer[0] = (char)(orig - c2);  // LV syllable
                buffer[1] = (char)(JamoTBase + c2);
            }
            return buffer.Slice(0, 2);
        }
    }

    #region ReorderingBuffer

    /// <summary>
    /// Writable buffer that takes care of canonical ordering.
    /// Its Append methods behave like the C++ implementation's
    /// appendZeroCC() methods.
    /// <para/>
    /// The buffer maintains a ValueStringBuilder for intermediate text segments
    /// until no further changes are necessary and whole segments are appended.
    /// When done editing, the value can be obtained by calling
    /// <see cref="TryCopyTo(Span{char}, out int)"/>, <see cref="AsSpan()"/>, or
    /// <see cref="ToString()"/>. The user is responsible for calling <see cref="Dispose()"/>
    /// if the value is not obtained through <see cref="ToString()"/>.
    /// </summary>
    public unsafe ref struct ReorderingBuffer
    {
        public ReorderingBuffer(Normalizer2Impl ni, Span<char> initialBuffer)
            : this(ni, ReadOnlySpan<char>.Empty, initialBuffer)
        {
        }
        public ReorderingBuffer(Normalizer2Impl ni, ReadOnlySpan<char> initialValue, Span<char> initialBuffer)
        {
            impl = ni ?? throw new ArgumentNullException(nameof(ni));
            str = new ValueStringBuilder(initialBuffer);
            if (!initialValue.IsEmpty)
            {
                str.Append(initialValue);
            }
            reorderStart = 0;
            codePointStart = 0;
            codePointLimit = 0;
            lastCC = 0;
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

        public ReorderingBuffer(Normalizer2Impl ni, int initialCapacity)
            : this(ni, ReadOnlySpan<char>.Empty, initialCapacity)
        {
        }

        // ICU4N TODO: Evaluate whether this approach makes sense and if not, remove
        public ReorderingBuffer(Normalizer2Impl ni, ReadOnlySpan<char> initialValue, int initialCapacity)
        {
            impl = ni ?? throw new ArgumentNullException(nameof(ni));
            str = new ValueStringBuilder(initialCapacity);
            if (!initialValue.IsEmpty)
            {
                str.Append(initialValue);
            }
            reorderStart = 0;
            codePointStart = 0;
            codePointLimit = 0;
            lastCC = 0;
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

        internal ReorderingBuffer(Normalizer2Impl ni, ref ValueStringBuilder destination, int destinationCapacity)
        {
            impl = ni ?? throw new ArgumentNullException(nameof(ni));
            str = destination;
            str.EnsureCapacity(destinationCapacity);
            reorderStart = 0;
            codePointStart = 0;
            codePointLimit = 0;
            lastCC = 0;
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


        public bool IsEmpty => str.Length == 0;
        public int Length => str.Length;
        public int LastCC => lastCC;

        // ICU4N: It is not possible to return ValueStringBuilder by ref here,
        // so we surface the part of the ValueStringBuilder API that Recompose() uses instead.
        internal ref char this[int index] => ref str[index];
        internal void Insert(int index, char value) => str.Insert(index, value);
        internal void Remove(int startIndex, int length) => str.Remove(startIndex, length);
        internal int CodePointAt(int index) => str.CodePointAt(index);

        public ReadOnlySpan<char> AsSpan() => str.AsSpan();
        public ReadOnlySpan<char> AsSpan(int start) => str.AsSpan(start);
        public ReadOnlySpan<char> AsSpan(int start, int length) => str.AsSpan(start, length);

        public Span<char> RawChars => str.RawChars;

        public bool TryCopyTo(Span<char> destination, out int charsWritten) => str.TryCopyTo(destination, out charsWritten);

        public override string ToString() => str.ToString();
        public void Dispose() => str.Dispose();


        public bool Equals(string s, int start, int length) // ICU4N specific: changed limit to length
        {
            return UTF16Plus.Equal(str.AsSpan(), s.AsSpan(start, length));
        }

        public bool Equals(scoped ReadOnlySpan<char> s)
        {
            return UTF16Plus.Equal(str.AsSpan(), s);
        }


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

        // s must be in NFD, otherwise change the implementation.
        public void Append(string s, int start, int length,
            int leadCC, int trailCC) // ICU4N specific: changed limit to length
        {
            if (s is null)
                throw new ArgumentNullException(nameof(s));

            Append(s.AsSpan(start, length), leadCC, trailCC);
        }

        // s must be in NFD, otherwise change the implementation.
        public void Append(StringBuilder s, int start, int length,
            int leadCC, int trailCC) // ICU4N specific: changed limit to length
        {
            if (s is null)
                throw new ArgumentNullException(nameof(s));

            ValueStringBuilder buffer = new ValueStringBuilder(stackalloc char[Normalizer2.CharStackBufferSize]);
            try
            {
                buffer.Append(s);
                Append(buffer.AsSpan(), leadCC, trailCC);
            }
            finally
            {
                buffer.Dispose();
            }
        }

        // s must be in NFD, otherwise change the implementation.
        public void Append(scoped ReadOnlySpan<char> s, int leadCC, int trailCC)
        {
            int start = 0, length = s.Length; // ICU4N: Removed from method signature because we can slice
            if (length == 0)
            {
                return;
            }
            if (lastCC <= leadCC || leadCC == 0)
            {
                if (trailCC <= 1)
                {
                    reorderStart = str.Length + length;
                }
                else if (leadCC <= 1)
                {
                    reorderStart = str.Length + 1;  // Ok if not a code point boundary.
                }
                str.Append(s); // ICU4N: removed start and length - we slice to get here
                lastCC = trailCC;
            }
            else
            {
                int limit = start + length;
                int c = Character.CodePointAt(s, start);
                start += Character.CharCount(c);
                Insert(c, leadCC);  // insert first code point
                while (start < limit)
                {
                    c = Character.CodePointAt(s, start);
                    start += Character.CharCount(c);
                    if (start < limit)
                    {
                        // s must be in NFD, otherwise we need to use getCC().
                        leadCC = Normalizer2Impl.GetCCFromYesOrMaybe(impl.GetNorm16(c));
                    }
                    else
                    {
                        leadCC = trailCC;
                    }
                    Append(c, leadCC);
                }
            }
        }


        // The following append() methods work like C++ appendZeroCC().
        // They assume that the cc or trailCC of their input is 0.
        // Most of them implement Appendable interface methods.
        public void Append(char c)
        {
            str.Append(c);
            lastCC = 0;
            reorderStart = str.Length;
        }

        public void AppendZeroCC(int c)
        {
            str.AppendCodePoint(c);
            lastCC = 0;
            reorderStart = str.Length;
        }

        public void Append(string? s)
        {
            if (s != null && s.Length != 0)
            {
                str.Append(s);
                lastCC = 0;
                reorderStart = str.Length;
            }
        }

        public void Append(StringBuilder? s)
        {
            if (s != null && s.Length != 0)
            {
                str.Append(s);
                lastCC = 0;
                reorderStart = str.Length;
            }
        }

        public void Append(scoped ReadOnlySpan<char> s)
        {
            if (s.Length != 0)
            {
                str.Append(s);
                lastCC = 0;
                reorderStart = str.Length;
            }
        }

        public void Append(string? s, int start, int length) // ICU4N specific: changed limit to length
        {
            if (length != 0)
            {
                str.Append(s, start, length); // ICU4N: checked 3rd parameter
                lastCC = 0;
                reorderStart = str.Length;
            }
        }

        public void Append(StringBuilder? s, int start, int length) // ICU4N specific: changed limit to length
        {
            if (length != 0)
            {
                str.Append(s, start, length); // ICU4N: checked 3rd parameter
                lastCC = 0;
                reorderStart = str.Length;
            }
        }

        /// <summary>
        /// Flushes from the intermediate <see cref="StringBuilder"/> to the <see cref="IAppendable"/>,
        /// if they are different objects.
        /// Used after recomposition.
        /// Must be called at the end when writing.
        /// </summary>
        public void Flush()
        {
            reorderStart = str.Length;
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

        // ICU4N NOTE: Instead of FlushAndAppendZeroCC(string, int, int), call Append(string, int, int)
        // ICU4N NOTE: Instead of FlushAndAppendZeroCC(StringBuilder, int, int), call Append(StringBuilder, int, int)
        // ICU4N NOTE: Instead of FlushAndAppendZeroCC(char[], int, int), call Append(char[], int, int)
        // ICU4N NOTE: Instead of FlushAndAppendZeroCC(ICharSequence, int, int), call Append(ICharSequence, int, int)
        // ICU4N NOTE: Instead of FlushAndAppendZeroCC(ReadOnlySpan<char>, int, int), call Append(ReadOnlySpan<char>, int, int)


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
                str.InsertCodePoint(codePointLimit, c);
                if (cc <= 1)
                {
                    reorderStart = codePointLimit + 2;
                }
            }
        }

        private /*readonly*/ Normalizer2Impl impl;
        private /*readonly*/ ValueStringBuilder str;
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


        private int codePointStart, codePointLimit;
    }

    #endregion ReorderingBuffer

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

        #region Equal(ICharSequence, ICharSequence)
        /// <summary>
        /// Compares two character sequence objects for binary equality.
        /// </summary>
        /// <param name="s1">s1 first sequence</param>
        /// <param name="s2">s2 second sequence</param>
        /// <returns>true if s1 contains the same text as s2.</returns>
        public static bool Equal(string s1, string s2)
        {
            if (s1 == s2)
            {
                return true;
            }
            if (s1 is null || s2 is null) return false;
            // ICU4N: Use optimized equality comparison in System.Memory
            return System.MemoryExtensions.Equals(s1.AsSpan(), s2.AsSpan(), StringComparison.Ordinal);
        }

        /// <summary>
        /// Compares two character sequence objects for binary equality.
        /// </summary>
        /// <param name="s1">s1 first sequence</param>
        /// <param name="s2">s2 second sequence</param>
        /// <returns>true if s1 contains the same text as s2.</returns>
        public static bool Equal(string s1, ReadOnlySpan<char> s2)
        {
            if (s1 is null) return false;
            // ICU4N: Use optimized equality comparison in System.Memory
            return System.MemoryExtensions.Equals(s1.AsSpan(), s2, StringComparison.Ordinal);
        }

        /// <summary>
        /// Compares two character sequence objects for binary equality.
        /// </summary>
        /// <param name="s1">s1 first sequence</param>
        /// <param name="s2">s2 second sequence</param>
        /// <returns>true if s1 contains the same text as s2.</returns>
        public static bool Equal(ReadOnlySpan<char> s1, string s2)
        {
            if (s2 is null) return false;
            // ICU4N: Use optimized equality comparison in System.Memory
            return System.MemoryExtensions.Equals(s1, s2.AsSpan(), StringComparison.Ordinal);
        }

        /// <summary>
        /// Compares two character sequence objects for binary equality.
        /// </summary>
        /// <param name="s1">s1 first sequence</param>
        /// <param name="s2">s2 second sequence</param>
        /// <returns>true if s1 contains the same text as s2.</returns>
        public static bool Equal(ReadOnlySpan<char> s1, ReadOnlySpan<char> s2)
        {
            // ICU4N: Use optimized equality comparison in System.Memory
            return System.MemoryExtensions.Equals(s1, s2, StringComparison.Ordinal);
        }

        #endregion Equal(ICharSequence, ICharSequence)

        // ICU4N specific -  Equal(ICharSequence s1, int start1, int limit1,
        //    ICharSequence s2, int start2, int limit2) factored out because we can slice
    }

    /// <summary>
    /// Low-level implementation of the Unicode Normalization Algorithm.
    /// For the data structure and details see the documentation at the end of
    /// C++ normalizer2impl.h and in the design doc at
    /// http://site.icu-project.org/design/normalization/custom
    /// </summary>
    public sealed partial class Normalizer2Impl
    {
        private const int CharStackBufferSize = 64;

        // ICU4N specific - de-nested Hangul class

        // ICU4N specific - de-nested ReorderingBuffer class

        // ICU4N specific - de-nested UTF16Plus class

#nullable disable
        // ICU4N TODO: Some fields are not populated here so we get nullable warnings. Disabling for now.
        public Normalizer2Impl() { }

#nullable enable

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
                if (indexesLength <= IxMinLcccCp)
                {
                    throw new ICUUncheckedIOException("Normalizer2 data: not enough indexes");
                }
                int[] inIndexes = new int[indexesLength];
                inIndexes[0] = indexesLength * 4;
                for (int i = 1; i < indexesLength; ++i)
                {
                    inIndexes[i] = bytes.GetInt32();
                }

                minDecompNoCP = inIndexes[IxMinDecompNoCp];
                minCompNoMaybeCP = inIndexes[IxMinCompNoMaybeCp];
                minLcccCP = inIndexes[IxMinLcccCp];

                minYesNo = inIndexes[IxMinYesNo];
                minYesNoMappingsOnly = inIndexes[IxMinYesNoMappingsOnly];
                minNoNo = inIndexes[IxMinNoNo];
                minNoNoCompBoundaryBefore = inIndexes[IxMinNoNoCompBoundaryBefore];
                minNoNoCompNoMaybeCC = inIndexes[IxMinNoNoCompNoMaybeCc];
                minNoNoEmpty = inIndexes[IxMinNoNoEmpty];
                limitNoNo = inIndexes[IxLimitNoNo];
                minMaybeYes = inIndexes[IxMinMaybeYes];
                Debug.Assert((minMaybeYes & 7) == 0);  // 8-aligned for noNoDelta bit fields
                centerNoNoDelta = (minMaybeYes >> DeltaShift) - MaxDelta - 1;

                // Read the normTrie.
                int offset = inIndexes[IxNormTrieOffset];
                int nextOffset = inIndexes[IxExtraDataOffset];
                normTrie = Trie2_16.CreateFromSerialized(bytes);
                int trieLength = normTrie.SerializedLength;
                if (trieLength > (nextOffset - offset))
                {
                    throw new ICUUncheckedIOException("Normalizer2 data: not enough bytes for normTrie");
                }
                ICUBinary.SkipBytes(bytes, (nextOffset - offset) - trieLength);  // skip padding after trie bytes

                // Read the composition and mapping data.
                offset = nextOffset;
                nextOffset = inIndexes[IxSmallFcdOffset];
                int numChars = (nextOffset - offset) / 2;
                if (numChars != 0)
                {
                    maybeYesCompositions = ICUBinary.GetString(bytes, numChars, 0);
                    extraData = maybeYesCompositions.Substring((MinNormalMaybeYes - minMaybeYes) >> OffsetShift);
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
            if (norm16 > MinNormalMaybeYes && norm16 != JamoVt)
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
            if (start != end && IsAlgorithmicNoNo(value) && (value & DeltaTcccMask) > DeltaTccc1)
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
        /// </summary>
        /// <returns>This.</returns>
        public Normalizer2Impl EnsureCanonIterData() // ICU4N: This is not reqired, as it was in ICU4J
        {
            if (canonIterData == null)
            {
                LazyInitializer.EnsureInitialized(ref canonIterData!, () =>
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
                                    if (norm16 < MinNormalMaybeYes)
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
                                        int mapping = norm16_2 >> OffsetShift;
                                        int firstUnit = extraData[mapping];
                                        int length = firstUnit & MappingLengthMask;
                                        if ((firstUnit & MappingHasCccLcccWord) != 0)
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
                    return newData.ToTrie2_32();
                });
            }

            return this;
        }

        public int GetNorm16(int c) { return normTrie.Get(c); }

        public int GetCompQuickCheck(int norm16)
        {
            if (norm16 < minNoNo || MinYesYesWithCc <= norm16)
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
            if (norm16 >= MinNormalMaybeYes)
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
            return (norm16 >> OffsetShift) & 0xff;
        }
        public static int GetCCFromYesOrMaybe(int norm16)
        {
            return norm16 >= MinNormalMaybeYes ? GetCCFromNormalYesOrMaybe(norm16) : 0;
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
                if (norm16 >= MinNormalMaybeYes)
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
                    int deltaTrailCC = norm16 & DeltaTcccMask;
                    if (deltaTrailCC <= DeltaTccc1)
                    {
                        return deltaTrailCC >> OffsetShift;
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
            int mapping = norm16 >> OffsetShift;
            int firstUnit = extraData[mapping];
            int fcd16 = firstUnit >> 8;  // tccc
            if ((firstUnit & MappingHasCccLcccWord) != 0)
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
        public string? GetDecomposition(int c)
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
                ValueStringBuilder buffer = new ValueStringBuilder(stackalloc char[4]);
                try
                {
                    buffer.AppendHangulDecomposition(c);
                    return buffer.ToString();
                }
                finally
                {
                    buffer.Dispose();
                }
            }
            // c decomposes, get everything from the variable-length extra data
            int mapping = norm16 >> OffsetShift;
            int length = extraData[mapping++] & MappingLengthMask;
            return extraData.Substring(mapping, length); // ICU4N: (mapping + length) - mapping == length
        }

        /// <summary>
        /// Gets the decomposition for one code point and writes it to <paramref name="destination"/>.
        /// </summary>
        /// <param name="c">Code point.</param>
        /// <param name="destination">Upon return, will contain the decomposition.</param>
        /// <param name="charsLength">Upon return, will contain the length of the decomposition (whether successuful or not).
        /// If the value is 0, it means there is not a valid decomposition value. If the value is greater than 0 and
        /// the method returns <c>false</c>, it means that there was not enough space allocated and the number indicates
        /// the minimum number of chars required.</param>
        /// <returns><c>true</c> if the decomposition was succssfully written to <paramref name="destination"/>; otherwise, <c>false</c>.</returns>
        public bool TryGetDecomposition(int c, Span<char> destination, out int charsLength) // ICU4N TODO: Tests
        {
            int norm16;
            if (c < minDecompNoCP || IsMaybeOrNonZeroCC(norm16 = GetNorm16(c)))
            {
                // c does not decompose
                charsLength = 0;
                return false;
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
                    charsLength = 0;
                    return false;
                }
                else
                {
                    charsLength = decomp < UTF16.SupplementaryMinValue ? 1 : 2;
                    if (destination.Length >= charsLength)
                    {
                        UTF16.ValueOf(decomp, destination, 0);
                        return true;
                    }
                    return false;
                }
            }
            else if (IsHangulLV(norm16) || IsHangulLVT(norm16))
            {
                // Hangul syllable: decompose algorithmically
                ValueStringBuilder buffer = new ValueStringBuilder(destination);
                try
                {
                    buffer.AppendHangulDecomposition(c);
                    return buffer.FitsInitialBuffer(out charsLength);
                }
                finally
                {
                    buffer.Dispose();
                }
            }
            // c decomposes, get everything from the variable-length extra data
            int mapping = norm16 >> OffsetShift;
            charsLength = extraData[mapping++] & MappingLengthMask;
            return extraData.AsSpan(mapping, charsLength).TryCopyTo(destination); // ICU4N: (mapping + length) - mapping == length
        }


        /// <summary>
        /// Gets the raw decomposition for one code point.
        /// </summary>
        /// <param name="c">Code point.</param>
        /// <returns><paramref name="c"/>'s raw decomposition, if it has one; returns null if it does not have a decomposition.</returns>
        public string? GetRawDecomposition(int c)
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
                ValueStringBuilder buffer = new ValueStringBuilder(stackalloc char[4]);
                try
                {
                    buffer.AppendHangulRawDecomposition(c);
                    return buffer.ToString();
                }
                finally
                {
                    buffer.Dispose();
                }
            }
            else if (IsDecompNoAlgorithmic(norm16))
            {
                return UTF16.ValueOf(MapAlgorithmic(c, norm16));
            }
            // c decomposes, get everything from the variable-length extra data
            int mapping = norm16 >> OffsetShift;
            int firstUnit = extraData[mapping];
            int mLength = firstUnit & MappingLengthMask;  // length of normal mapping
            if ((firstUnit & MappingHasRawMapping) != 0)
            {
                // Read the raw mapping from before the firstUnit and before the optional ccc/lccc word.
                // Bit 7=MAPPING_HAS_CCC_LCCC_WORD
                int rawMapping = mapping - ((firstUnit >> 7) & 1) - 1;
                char rm0 = extraData[rawMapping];
                if (rm0 <= MappingLengthMask)
                {
                    return extraData.Substring(rawMapping - rm0, rm0); // ICU4N: (rawMapping - rm0) - rawMapping == rm0
                }
                else
                {
                    // Copy the normal mapping and replace its first two code units with rm0.
                    using ValueStringBuilder buffer = new ValueStringBuilder(stackalloc char[mLength - 1]);
                    buffer.Append(rm0);
                    mapping += 1 + 2;  // skip over the firstUnit and the first two mapping code units
                    buffer.Append(extraData, mapping, mLength - 2); // (mapping + mLength - 2) - mapping == mLength - 2
                    return buffer.ToString();
                }
            }
            else
            {
                mapping += 1;  // skip over the firstUnit
                return extraData.Substring(mapping, mLength); // ICU4N: (mapping + mLength) - mapping == mLength
            }
        }

        /// <summary>
        /// Gets the raw decomposition for one code point and writes it to <paramref name="destination"/>.
        /// </summary>
        /// <param name="c">Code point.</param>
        /// <param name="destination">Upon return, will contain the raw decomposition.</param>
        /// <param name="charsLength">Upon return, will contain the length of the decomposition (whether successuful or not).
        /// If the value is 0, it means there is not a valid decomposition value. If the value is greater than 0 and
        /// the method returns <c>false</c>, it means that there was not enough space allocated and the number indicates
        /// the minimum number of chars required.</param>
        /// <returns><c>true</c> if the decomposition was succssfully written to <paramref name="destination"/>; otherwise, <c>false</c>.</returns>
        public bool TryGetRawDecomposition(int c, Span<char> destination, out int charsLength) // ICU4N TODO: Tests
        {
            int norm16;
            if (c < minDecompNoCP || IsDecompYes(norm16 = GetNorm16(c)))
            {
                // c does not decompose
                charsLength = 0;
                return false;
            }
            else if (IsHangulLV(norm16) || IsHangulLVT(norm16))
            {
                // Hangul syllable: decompose algorithmically
                ValueStringBuilder buffer = new ValueStringBuilder(destination);
                try
                {
                    buffer.AppendHangulRawDecomposition(c);
                    return buffer.FitsInitialBuffer(out charsLength);
                }
                finally
                {
                    buffer.Dispose();
                }
            }
            else if (IsDecompNoAlgorithmic(norm16))
            {
                int mapped = MapAlgorithmic(c, norm16);
                charsLength = mapped < UTF16.SupplementaryMinValue ? 1 : 2;
                if (destination.Length >= charsLength)
                {
                    UTF16.ValueOf(mapped, destination, 0);
                    return true;
                }
                return false;
            }
            // c decomposes, get everything from the variable-length extra data
            int mapping = norm16 >> OffsetShift;
            int firstUnit = extraData[mapping];
            int mLength = firstUnit & MappingLengthMask;  // length of normal mapping
            if ((firstUnit & MappingHasRawMapping) != 0)
            {
                // Read the raw mapping from before the firstUnit and before the optional ccc/lccc word.
                // Bit 7=MAPPING_HAS_CCC_LCCC_WORD
                int rawMapping = mapping - ((firstUnit >> 7) & 1) - 1;
                char rm0 = extraData[rawMapping];
                if (rm0 <= MappingLengthMask)
                {
                    charsLength = rm0;
                    return extraData.AsSpan(rawMapping - rm0, rm0).TryCopyTo(destination); // ICU4N: (rawMapping - rm0) - rawMapping == rm0
                }
                else
                {
                    // Copy the normal mapping and replace its first two code units with rm0.
                    using ValueStringBuilder buffer = new ValueStringBuilder(stackalloc char[mLength - 1]);
                    buffer.Append(rm0);
                    mapping += 1 + 2;  // skip over the firstUnit and the first two mapping code units
                    buffer.Append(extraData.AsSpan(mapping, mLength - 2));
                    charsLength = mLength - 1;
                    return buffer.TryCopyTo(destination, out _);
                }
            }
            else
            {
                mapping += 1;  // skip over the firstUnit
                charsLength = mLength;
                return extraData.AsSpan(mapping, mLength).TryCopyTo(destination); // ICU4N: (mapping + mLength) - mapping == mLength
            }
        }

        /// <summary>
        /// Returns true if code point <paramref name="c"/> starts a canonical-iterator string segment.
        /// </summary>
        /// <param name="c">A Unicode code point.</param>
        /// <returns>true if <paramref name="c"/> starts a canonical-iterator string segment.</returns>
        public bool IsCanonSegmentStarter(int c)
        {
            EnsureCanonIterData(); // ICU4N: Make this call automatically, so the user doesn't have to bother with it.
            return canonIterData.Get(c) >= 0;
        }

        /// <summary>
        /// Returns true if there are characters whose decomposition starts with <paramref name="c"/>.
        /// If so, then the set is cleared and then filled with those characters.
        /// </summary>
        /// <param name="c">A Unicode code point.</param>
        /// <param name="set">A <see cref="UnicodeSet"/> to receive the characters whose decompositions
        /// start with <paramref name="c"/>, if there are any.</param>
        /// <returns>true if there are characters whose decomposition starts with <paramref name="c"/>.</returns>
        public bool GetCanonStartSet(int c, UnicodeSet set)
        {
            EnsureCanonIterData(); // ICU4N: Make this call automatically, so the user doesn't have to bother with it.
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
                if (norm16 == JamoL)
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

        // Fixed norm16 values.
        public const int MinYesYesWithCc = 0xfe02;
        public const int JamoVt = 0xfe00;
        public const int MinNormalMaybeYes = 0xfc00;
        public const int JamoL = 2;  // offset=1 hasCompBoundaryAfter=FALSE
        public const int Inert = 1;  // offset=0 hasCompBoundaryAfter=TRUE

        // norm16 bit 0 is comp-boundary-after.
        public const int HasCompBoundaryAfterValue = 1; // ICU4N-specific - added "Value" suffix to not conflict with method
        public const int OffsetShift = 1;

        // For algorithmic one-way mappings, norm16 bits 2..1 indicate the
        // tccc (0, 1, >1) for quick FCC boundary-after tests.
        public const int DeltaTccc0 = 0;
        public const int DeltaTccc1 = 2;
        public const int DeltaTcccGt1 = 4;
        public const int DeltaTcccMask = 6;
        public const int DeltaShift = 3;

        public const int MaxDelta = 0x40;

        // Byte offsets from the start of the data, after the generic header.
        public const int IxNormTrieOffset = 0;
        public const int IxExtraDataOffset = 1;
        public const int IxSmallFcdOffset = 2;
        public const int IxReserved3Offset = 3;
        public const int IxTotalSize = 7;

        // Code point thresholds for quick check codes.
        public const int IxMinDecompNoCp = 8;
        public const int IxMinCompNoMaybeCp = 9;

        // Norm16 value thresholds for quick check combinations and types of extra data.

        /// <summary>Mappings &amp; compositions in [minYesNo..minYesNoMappingsOnly[.</summary>
        public const int IxMinYesNo = 10;
        /// <summary>Mappings are comp-normalized.</summary>
        public const int IxMinNoNo = 11;
        public const int IxLimitNoNo = 12;
        public const int IxMinMaybeYes = 13;

        /// <summary>Mappings only in [minYesNoMappingsOnly..minNoNo[.</summary>
        public const int IxMinYesNoMappingsOnly = 14;
        /// <summary>Mappings are not comp-normalized but have a comp boundary before.</summary>
        public const int IxMinNoNoCompBoundaryBefore = 15;
        /// <summary>Mappings do not have a comp boundary before.</summary>
        public const int IxMinNoNoCompNoMaybeCc = 16;
        /// <summary>Mappings to the empty string.</summary>
        public const int IxMinNoNoEmpty = 17;

        public const int IxMinLcccCp = 18;
        public const int IxCount = 20;

        public const int MappingHasCccLcccWord = 0x80;
        public const int MappingHasRawMapping = 0x40;
        // unused bit 0x20;
        public const int MappingLengthMask = 0x1f;

        public const int Comp1LastTuple = 0x8000;
        public const int Comp1Triple = 1;
        public const int Comp1TrailLimit = 0x3400;
        public const int Comp1TrailMask = 0x7ffe;
        public const int Comp1TrailShift = 9;  // 10-1 for the "triple" bit
        public const int Comp2TrailShift = 6;
        public const int Comp2TrailMask = 0xffc0;

        // higher-level functionality ------------------------------------------ ***

        // NFD without an NFD Normalizer2 instance.
        public StringBuilder Decompose(ReadOnlySpan<char> s, StringBuilder dest)
        {
            Decompose(s, dest, s.Length);
            return dest;
        }

        // ICU4N TODO: Make public TryDecompose() method that accepts ReadOnlySpan<char>, Span<char>, out int charLength
        internal void Decompose(scoped ReadOnlySpan<char> s, scoped ref ValueStringBuilder dest) // ICU4N: internal because ValueStringBuilder is internal
        {
            Decompose(s, ref dest, s.Length);
        }

        /// <summary>
        /// Decomposes s[src, length[ and writes the result to <paramref name="dest"/>.
        /// length can be NULL if src is NUL-terminated.
        /// <paramref name="destLengthEstimate"/> is the initial <paramref name="dest"/> buffer capacity and can be -1.
        /// </summary>
        public void Decompose(ReadOnlySpan<char> s, StringBuilder dest, int destLengthEstimate)
        {
            var sb = destLengthEstimate <= CharStackBufferSize
                ? new ValueStringBuilder(stackalloc char[CharStackBufferSize])
                : new ValueStringBuilder(destLengthEstimate);

            try
            {
                Decompose(s, ref sb, destLengthEstimate);
                dest.Append(sb.AsSpan());
            }
            finally
            {
                sb.Dispose();
            }
        }

        /// <summary>
        /// Decomposes s[src, length[ and writes the result to <paramref name="dest"/>.
        /// length can be NULL if src is NUL-terminated.
        /// <paramref name="destLengthEstimate"/> is the initial <paramref name="dest"/> buffer capacity and can be -1.
        /// </summary>
        internal void Decompose(scoped ReadOnlySpan<char> s, scoped ref ValueStringBuilder dest, int destLengthEstimate)
        {
            int src = 0, limit = s.Length;
            if (destLengthEstimate < 0)
            {
                destLengthEstimate = limit - src;
            }
            ReorderingBuffer buffer = new ReorderingBuffer(this, ref dest, destLengthEstimate);
            dest.Length = 0;
            Decompose(s, ref buffer);
            dest.Length = buffer.Length; // HACK: Although the value gets written to dest, the length value needs to be manually transferred.
        }

        // normalize
        // ICU4N: This was part of the dual functionality of Decompose() in ICU4J.
        // Separated out into Decompose() and DecomposeQuickCheck() so we can use a ref struct for the buffer.
        public int Decompose(scoped ReadOnlySpan<char> s, scoped ref ReorderingBuffer buffer)
        {
            int src = 0, limit = s.Length;
            int minNoCP = minDecompNoCP;

            int prevSrc;
            int c = 0;
            int norm16 = 0;


            for (; ; )
            {
                // count code units below the minimum or with irrelevant data for the quick check
                for (prevSrc = src; src != limit;)
                {
                    if ((c = s[src]) < minNoCP ||
                        IsMostDecompYesAndZeroCC(norm16 = normTrie.GetFromU16SingleLead((char)c))
                    )
                    {
                        ++src;
                    }
                    else if (!UTF16.IsSurrogate((char)c))
                    {
                        break;
                    }
                    else
                    {
                        char c2;
                        if (UTF16Plus.IsSurrogateLead(c))
                        {
                            if ((src + 1) != limit && char.IsLowSurrogate(c2 = s[src + 1]))
                            {
                                c = Character.ToCodePoint((char)c, c2);
                            }
                        }
                        else /* trail surrogate */
                        {
                            if (prevSrc < src && char.IsHighSurrogate(c2 = s[src - 1]))
                            {
                                --src;
                                c = Character.ToCodePoint(c2, (char)c);
                            }
                        }
                        if (IsMostDecompYesAndZeroCC(norm16 = GetNorm16(c)))
                        {
                            src += Character.CharCount(c);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                // copy these code units all at once
                if (src != prevSrc)
                {
                    // For ReorderingBuffer, call Append() instead of FlushAndAppendZeroCC()
                    buffer.Append(s.Slice(prevSrc, src - prevSrc)); // ICU4N: Corrected 3rd parameter
                }
                if (src == limit)
                {
                    break;
                }

                // Check one above-minimum, relevant code point.
                src += Character.CharCount(c);
                Decompose(c, norm16, ref buffer);
            }
            return src;
        }

        // normalize
        // ICU4N: This was part of the dual functionality of Decompose() in ICU4J.
        // Separated out into Decompose() and DecomposeQuickCheck() so we can use a ref struct for the buffer.
        public int DecomposeQuickCheck(ReadOnlySpan<char> s)
        {
            int src = 0, limit = s.Length;
            int minNoCP = minDecompNoCP;

            int prevSrc;
            int c = 0;
            int norm16 = 0;

            int prevBoundary = src;
            int prevCC = 0;

            for (; ; )
            {
                // count code units below the minimum or with irrelevant data for the quick check
                for (prevSrc = src; src != limit;)
                {
                    if ((c = s[src]) < minNoCP ||
                        IsMostDecompYesAndZeroCC(norm16 = normTrie.GetFromU16SingleLead((char)c))
                    )
                    {
                        ++src;
                    }
                    else if (!UTF16.IsSurrogate((char)c))
                    {
                        break;
                    }
                    else
                    {
                        char c2;
                        if (UTF16Plus.IsSurrogateLead(c))
                        {
                            if ((src + 1) != limit && char.IsLowSurrogate(c2 = s[src + 1]))
                            {
                                c = Character.ToCodePoint((char)c, c2);
                            }
                        }
                        else /* trail surrogate */
                        {
                            if (prevSrc < src && char.IsHighSurrogate(c2 = s[src - 1]))
                            {
                                --src;
                                c = Character.ToCodePoint(c2, (char)c);
                            }
                        }
                        if (IsMostDecompYesAndZeroCC(norm16 = GetNorm16(c)))
                        {
                            src += Character.CharCount(c);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                // copy these code units all at once
                if (src != prevSrc)
                {
                    prevCC = 0;
                    prevBoundary = src;
                }
                if (src == limit)
                {
                    break;
                }

                // Check one above-minimum, relevant code point.
                src += Character.CharCount(c);
                if (IsDecompYes(norm16))
                {
                    int cc = GetCCFromYesOrMaybe(norm16);
                    if (prevCC <= cc || cc == 0)
                    {
                        prevCC = cc;
                        if (cc <= 1)
                        {
                            prevBoundary = src;
                        }
                        continue;
                    }
                }
                return prevBoundary;  // "no" or cc out of order
            }
            return src;
        }

        public void DecomposeAndAppend(scoped ReadOnlySpan<char> s, bool doDecompose, scoped ref ReorderingBuffer buffer)
        {
            int limit = s.Length;
            if (limit == 0)
            {
                return;
            }
            if (doDecompose)
            {
                Decompose(s, ref buffer);
                return;
            }
            // Just merge the strings at the boundary.
            int c = Character.CodePointAt(s, 0);
            int src = 0;
            int firstCC, prevCC, cc;
            firstCC = prevCC = cc = GetCC(GetNorm16(c));
            while (cc != 0)
            {
                prevCC = cc;
                src += Character.CharCount(c);
                if (src >= limit)
                {
                    break;
                }
                c = Character.CodePointAt(s, src);
                cc = GetCC(GetNorm16(c));
            };
            buffer.Append(s.Slice(0, src - 0), firstCC, prevCC); // ICU4N: Corrected 3rd parameter
            buffer.Append(s.Slice(src, limit - src)); // ICU4N: Corrected 3rd parameter
        }

        // Very similar to ComposeQuickCheck(): Make the same changes in both places if relevant.
        // doCompose: normalize
        // !doCompose: isNormalized (buffer must be empty and initialized)
        public bool Compose(scoped ReadOnlySpan<char> s,
                           bool onlyContiguous,
                           bool doCompose,
                           scoped ref ReorderingBuffer buffer)
        {
            int src = 0, limit = s.Length;
            int prevBoundary = src;
            int minNoMaybeCP = minCompNoMaybeCP;

            for (; ; )
            {
                // Fast path: Scan over a sequence of characters below the minimum "no or maybe" code point,
                // or with (compYes && ccc==0) properties.
                int prevSrc;
                int c = 0;
                int norm16 = 0;
                for (; ; )
                {
                    if (src == limit)
                    {
                        if (prevBoundary != limit && doCompose)
                        {
                            buffer.Append(s.Slice(prevBoundary, limit - prevBoundary)); // ICU4N: Corrected 3rd parameter
                        }
                        return true;
                    }
                    if ((c = s[src]) < minNoMaybeCP ||
                        IsCompYesAndZeroCC(norm16 = normTrie.GetFromU16SingleLead((char)c))
                    )
                    {
                        ++src;
                    }
                    else
                    {
                        prevSrc = src++;
                        if (!UTF16.IsSurrogate((char)c))
                        {
                            break;
                        }
                        else
                        {
                            char c2;
                            if (UTF16Plus.IsSurrogateLead(c))
                            {
                                if (src != limit && char.IsLowSurrogate(c2 = s[src]))
                                {
                                    ++src;
                                    c = Character.ToCodePoint((char)c, c2);
                                }
                            }
                            else /* trail surrogate */
                            {
                                if (prevBoundary < prevSrc && char.IsHighSurrogate(c2 = s[prevSrc - 1]))
                                {
                                    --prevSrc;
                                    c = Character.ToCodePoint(c2, (char)c);
                                }
                            }
                            if (!IsCompYesAndZeroCC(norm16 = GetNorm16(c)))
                            {
                                break;
                            }
                        }
                    }
                }
                // isCompYesAndZeroCC(norm16) is false, that is, norm16>=minNoNo.
                // The current character is either a "noNo" (has a mapping)
                // or a "maybeYes" (combines backward)
                // or a "yesYes" with ccc!=0.
                // It is not a Hangul syllable or Jamo L because those have "yes" properties.

                // Medium-fast path: Handle cases that do not require full decomposition and recomposition.
                if (!IsMaybeOrNonZeroCC(norm16))
                {  // minNoNo <= norm16 < minMaybeYes
                    if (!doCompose)
                    {
                        return false;
                    }
                    // Fast path for mapping a character that is immediately surrounded by boundaries.
                    // In this case, we need not decompose around the current character.
                    if (IsDecompNoAlgorithmic(norm16))
                    {
                        // Maps to a single isCompYesAndZeroCC character
                        // which also implies hasCompBoundaryBefore.
                        if (Norm16HasCompBoundaryAfter(norm16, onlyContiguous) ||
                                HasCompBoundaryBefore(s, src, limit))
                        {
                            if (prevBoundary != prevSrc)
                            {
                                buffer.Append(s.Slice(prevBoundary, prevSrc - prevBoundary)); // ICU4N: Corrected 3rd parameter
                            }
                            buffer.Append(MapAlgorithmic(c, norm16), 0);
                            prevBoundary = src;
                            continue;
                        }
                    }
                    else if (norm16 < minNoNoCompBoundaryBefore)
                    {
                        // The mapping is comp-normalized which also implies hasCompBoundaryBefore.
                        if (Norm16HasCompBoundaryAfter(norm16, onlyContiguous) ||
                                HasCompBoundaryBefore(s, src, limit))
                        {
                            if (prevBoundary != prevSrc)
                            {
                                buffer.Append(s.Slice(prevBoundary, prevSrc - prevBoundary)); // ICU4N: Corrected 3rd parameter
                            }
                            int mapping = norm16 >> OffsetShift;
                            int length2 = extraData[mapping++] & MappingLengthMask;
                            buffer.Append(extraData, mapping, length2); // ICU4N: Corrected 3rd parameter
                            prevBoundary = src;
                            continue;
                        }
                    }
                    else if (norm16 >= minNoNoEmpty)
                    {
                        // The current character maps to nothing.
                        // Simply omit it from the output if there is a boundary before _or_ after it.
                        // The character itself implies no boundaries.
                        if (HasCompBoundaryBefore(s, src, limit) ||
                                HasCompBoundaryAfter(s, prevBoundary, prevSrc, onlyContiguous))
                        {
                            if (prevBoundary != prevSrc)
                            {
                                buffer.Append(s.Slice(prevBoundary, prevSrc - prevBoundary)); // ICU4N: Corrected 3rd parameter
                            }
                            prevBoundary = src;
                            continue;
                        }
                    }
                    // Other "noNo" type, or need to examine more text around this character:
                    // Fall through to the slow path.
                }
                else if (IsJamoVT(norm16) && prevBoundary != prevSrc)
                {
                    char prev = s[prevSrc - 1];
                    if (c < Hangul.JamoTBase)
                    {
                        // The current character is a Jamo Vowel,
                        // compose with previous Jamo L and following Jamo T.
                        char l = (char)(prev - Hangul.JamoLBase);
                        if (l < Hangul.JamoLCount)
                        {
                            if (!doCompose)
                            {
                                return false;
                            }
                            int t;
                            if (src != limit &&
                                    0 < (t = (s[src] - Hangul.JamoTBase)) &&
                                    t < Hangul.JamoTCount)
                            {
                                // The next character is a Jamo T.
                                ++src;
                            }
                            else if (HasCompBoundaryBefore(s, src, limit))
                            {
                                // No Jamo T follows, not even via decomposition.
                                t = 0;
                            }
                            else
                            {
                                t = -1;
                            }
                            if (t >= 0)
                            {
                                int syllable = Hangul.HangulBase +
                                    (l * Hangul.JamoVCount + (c - Hangul.JamoVBase)) *
                                    Hangul.JamoTCount + t;
                                --prevSrc;  // Replace the Jamo L as well.
                                if (prevBoundary != prevSrc)
                                {
                                    buffer.Append(s.Slice(prevBoundary, prevSrc - prevBoundary)); // ICU4N: Corrected 3rd parameter
                                }
                                buffer.Append((char)syllable);
                                prevBoundary = src;
                                continue;
                            }
                            // If we see L+V+x where x!=T then we drop to the slow path,
                            // decompose and recompose.
                            // This is to deal with NFKC finding normal L and V but a
                            // compatibility variant of a T.
                            // We need to either fully compose that combination here
                            // (which would complicate the code and may not work with strange custom data)
                            // or use the slow path.
                        }
                    }
                    else if (Hangul.IsHangulLV(prev))
                    {
                        // The current character is a Jamo Trailing consonant,
                        // compose with previous Hangul LV that does not contain a Jamo T.
                        if (!doCompose)
                        {
                            return false;
                        }
                        int syllable = prev + c - Hangul.JamoTBase;
                        --prevSrc;  // Replace the Hangul LV as well.
                        if (prevBoundary != prevSrc)
                        {
                            buffer.Append(s.Slice(prevBoundary, prevSrc - prevBoundary)); // ICU4N: Corrected 3rd parameter
                        }
                        buffer.Append((char)syllable);
                        prevBoundary = src;
                        continue;
                    }
                    // No matching context, or may need to decompose surrounding text first:
                    // Fall through to the slow path.
                }
                else if (norm16 > JamoVt)
                {  // norm16 >= MIN_YES_YES_WITH_CC
                   // One or more combining marks that do not combine-back:
                   // Check for canonical order, copy unchanged if ok and
                   // if followed by a character with a boundary-before.
                    int cc = GetCCFromNormalYesOrMaybe(norm16);  // cc!=0
                    if (onlyContiguous /* FCC */ && GetPreviousTrailCC(s, prevBoundary, prevSrc) > cc)
                    {
                        // Fails FCD test, need to decompose and contiguously recompose.
                        if (!doCompose)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        // If !onlyContiguous (not FCC), then we ignore the tccc of
                        // the previous character which passed the quick check "yes && ccc==0" test.
                        int n16;
                        for (; ; )
                        {
                            if (src == limit)
                            {
                                if (doCompose)
                                {
                                    buffer.Append(s.Slice(prevBoundary, limit - prevBoundary)); // ICU4N: Corrected 3rd parameter
                                }
                                return true;
                            }
                            int prevCC = cc;
                            c = Character.CodePointAt(s, src);
                            n16 = normTrie.Get(c);
                            if (n16 >= MinYesYesWithCc)
                            {
                                cc = GetCCFromNormalYesOrMaybe(n16);
                                if (prevCC > cc)
                                {
                                    if (!doCompose)
                                    {
                                        return false;
                                    }
                                    break;
                                }
                            }
                            else
                            {
                                break;
                            }
                            src += Character.CharCount(c);
                        }
                        // p is after the last in-order combining mark.
                        // If there is a boundary here, then we continue with no change.
                        if (Norm16HasCompBoundaryBefore(n16))
                        {
                            if (IsCompYesAndZeroCC(n16))
                            {
                                src += Character.CharCount(c);
                            }
                            continue;
                        }
                        // Use the slow path. There is no boundary in [prevSrc, src[.
                    }
                }

                // Slow path: Find the nearest boundaries around the current character,
                // decompose and recompose.
                if (prevBoundary != prevSrc && !Norm16HasCompBoundaryBefore(norm16))
                {
                    c = Character.CodePointBefore(s, prevSrc);
                    norm16 = normTrie.Get(c);
                    if (!Norm16HasCompBoundaryAfter(norm16, onlyContiguous))
                    {
                        prevSrc -= Character.CharCount(c);
                    }
                }
                if (doCompose && prevBoundary != prevSrc)
                {
                    buffer.Append(s.Slice(prevBoundary, prevSrc - prevBoundary)); // ICU4N: Corrected 3rd parameter
                }
                int recomposeStartIndex = buffer.Length;
                // We know there is not a boundary here.
                DecomposeShort(s, prevSrc, src, false /* !stopAtCompBoundary */, onlyContiguous,
                               ref buffer);
                // Decompose until the next boundary.
                src = DecomposeShort(s, src, limit, true /* stopAtCompBoundary */, onlyContiguous,
                                     ref buffer);
                Recompose(ref buffer, recomposeStartIndex, onlyContiguous);
                if (!doCompose)
                {
                    if (!buffer.Equals(s.Slice(prevSrc, src - prevSrc))) // ICU4N: Corrected 3rd parameter
                    {
                        return false;
                    }
                    buffer.Remove();
                }
                prevBoundary = src;
            }
        }

        /// <summary>
        /// Very similar to Compose(): Make the same changes in both places if relevant.
        /// doSpan: SpanQuickCheckYes (ignore bit 0 of the return value)
        /// !doSpan: QuickCheck
        /// </summary>
        /// <returns>
        /// bits 31..1: SpanQuickCheckYes (==s.Length if "yes") and
        /// bit 0: set if "maybe"; otherwise, if the span length&lt;s.Length
        /// then the quick check result is "no"
        /// </returns>
        public int ComposeQuickCheck(ReadOnlySpan<char> s,
            bool onlyContiguous, bool doSpan)
        {
            int src = 0, limit = s.Length;
            int qcResult = 0;
            int prevBoundary = src;
            int minNoMaybeCP = minCompNoMaybeCP;

            for (; ; )
            {
                // Fast path: Scan over a sequence of characters below the minimum "no or maybe" code point,
                // or with (compYes && ccc==0) properties.
                int prevSrc;
                int c = 0;
                int norm16 = 0;
                for (; ; )
                {
                    if (src == limit)
                    {
                        return (src << 1) | qcResult;  // "yes" or "maybe"
                    }
                    if ((c = s[src]) < minNoMaybeCP ||
                        IsCompYesAndZeroCC(norm16 = normTrie.GetFromU16SingleLead((char)c))
                    )
                    {
                        ++src;
                    }
                    else
                    {
                        prevSrc = src++;
                        if (!UTF16.IsSurrogate((char)c))
                        {
                            break;
                        }
                        else
                        {
                            char c2;
                            if (UTF16Plus.IsSurrogateLead(c))
                            {
                                if (src != limit && char.IsLowSurrogate(c2 = s[src]))
                                {
                                    ++src;
                                    c = Character.ToCodePoint((char)c, c2);
                                }
                            }
                            else /* trail surrogate */
                            {
                                if (prevBoundary < prevSrc && char.IsHighSurrogate(c2 = s[prevSrc - 1]))
                                {
                                    --prevSrc;
                                    c = Character.ToCodePoint(c2, (char)c);
                                }
                            }
                            if (!IsCompYesAndZeroCC(norm16 = GetNorm16(c)))
                            {
                                break;
                            }
                        }
                    }
                }
                // isCompYesAndZeroCC(norm16) is false, that is, norm16>=minNoNo.
                // The current character is either a "noNo" (has a mapping)
                // or a "maybeYes" (combines backward)
                // or a "yesYes" with ccc!=0.
                // It is not a Hangul syllable or Jamo L because those have "yes" properties.

                int prevNorm16 = Inert;
                if (prevBoundary != prevSrc)
                {
                    prevBoundary = prevSrc;
                    if (!Norm16HasCompBoundaryBefore(norm16))
                    {
                        c = Character.CodePointBefore(s, prevSrc);
                        int n16 = GetNorm16(c);
                        if (!Norm16HasCompBoundaryAfter(n16, onlyContiguous))
                        {
                            prevBoundary -= Character.CharCount(c);
                            prevNorm16 = n16;
                        }
                    }
                }

                if (IsMaybeOrNonZeroCC(norm16))
                {
                    int cc = GetCCFromYesOrMaybe(norm16);
                    if (onlyContiguous /* FCC */ && cc != 0 &&
                            GetTrailCCFromCompYesAndZeroCC(prevNorm16) > cc)
                    {
                        // The [prevBoundary..prevSrc[ character
                        // passed the quick check "yes && ccc==0" test
                        // but is out of canonical order with the current combining mark.
                    }
                    else
                    {
                        // If !onlyContiguous (not FCC), then we ignore the tccc of
                        // the previous character which passed the quick check "yes && ccc==0" test.
                        for (; ; )
                        {
                            if (norm16 < MinYesYesWithCc)
                            {
                                if (!doSpan)
                                {
                                    qcResult = 1;
                                }
                                else
                                {
                                    return prevBoundary << 1;  // spanYes does not care to know it's "maybe"
                                }
                            }
                            if (src == limit)
                            {
                                return (src << 1) | qcResult;  // "yes" or "maybe"
                            }
                            int prevCC = cc;
                            c = Character.CodePointAt(s, src);
                            norm16 = GetNorm16(c);
                            if (IsMaybeOrNonZeroCC(norm16))
                            {
                                cc = GetCCFromYesOrMaybe(norm16);
                                if (!(prevCC <= cc || cc == 0))
                                {
                                    break;
                                }
                            }
                            else
                            {
                                break;
                            }
                            src += Character.CharCount(c);
                        }
                        // src is after the last in-order combining mark.
                        if (IsCompYesAndZeroCC(norm16))
                        {
                            prevBoundary = src;
                            src += Character.CharCount(c);
                            continue;
                        }
                    }
                }
                return prevBoundary << 1;  // "no"
            }
        }

        public void ComposeAndAppend(scoped ReadOnlySpan<char> s,
            bool doCompose,
            bool onlyContiguous,
            scoped ref ReorderingBuffer buffer)
        {
            int src = 0, limit = s.Length;
            if (!buffer.IsEmpty)
            {
                int firstStarterInSrc = FindNextCompBoundary(s, 0, limit, onlyContiguous);
                if (0 != firstStarterInSrc)
                {
                    int lastStarterInDest = FindPreviousCompBoundary(buffer.AsSpan(),
                                                                   buffer.Length, onlyContiguous);
                    int middleLength = (buffer.Length - lastStarterInDest) + firstStarterInSrc + 16;
                    ValueStringBuilder middle = middleLength <= CharStackBufferSize
                        ? new ValueStringBuilder(stackalloc char[middleLength])
                        : new ValueStringBuilder(middleLength);
                    try
                    {
                        middle.Append(buffer.AsSpan(lastStarterInDest, buffer.Length - lastStarterInDest)); // ICU4N : Fixed length parameter
                        buffer.RemoveSuffix(buffer.Length - lastStarterInDest);
                        middle.Append(s.Slice(0, firstStarterInSrc - 0));
                        Compose(middle.AsSpan(), onlyContiguous, true, ref buffer);
                        src = firstStarterInSrc;
                    }
                    finally
                    {
                        middle.Dispose();
                    }
                }
            }
            if (doCompose)
            {
                Compose(s.Slice(src, limit - src), onlyContiguous, true, ref buffer); // ICU4N: Corrected length parameter
            }
            else
            {
                buffer.Append(s.Slice(src, limit - src)); // ICU4N: Corrected 3rd parameter
            }
        }

        // normalize
        // ICU4N: Separated dual functionality that was in ICU4J into MakeFCD() and MakeFCDSpanQuickCheckYes()
        public int MakeFCD(scoped ReadOnlySpan<char> s, scoped ref ReorderingBuffer buffer)
        {
            // Note: In this function we use buffer->appendZeroCC() because we track
            // the lead and trail combining classes here, rather than leaving it to
            // the ReorderingBuffer.
            // The exception is the call to decomposeShort() which uses the buffer
            // in the normal way.

            int src = 0, limit = s.Length;

            // Tracks the last FCD-safe boundary, before lccc=0 or after properly-ordered tccc<=1.
            // Similar to the prevBoundary in the compose() implementation.
            int prevBoundary = src;
            int prevSrc;
            int c = 0;
            int prevFCD16 = 0;
            int fcd16 = 0;

            for (; ; )
            {
                // count code units with lccc==0
                for (prevSrc = src; src != limit;)
                {
                    if ((c = s[src]) < minLcccCP)
                    {
                        prevFCD16 = ~c;
                        ++src;
                    }
                    else if (!SingleLeadMightHaveNonZeroFCD16(c))
                    {
                        prevFCD16 = 0;
                        ++src;
                    }
                    else
                    {
                        if (UTF16.IsSurrogate((char)c))
                        {
                            char c2;
                            if (UTF16Plus.IsSurrogateLead(c))
                            {
                                if ((src + 1) != limit && char.IsLowSurrogate(c2 = s[src + 1]))
                                {
                                    c = Character.ToCodePoint((char)c, c2);
                                }
                            }
                            else /* trail surrogate */
                            {
                                if (prevSrc < src && char.IsHighSurrogate(c2 = s[src - 1]))
                                {
                                    --src;
                                    c = Character.ToCodePoint(c2, (char)c);
                                }
                            }
                        }
                        if ((fcd16 = GetFCD16FromNormData(c)) <= 0xff)
                        {
                            prevFCD16 = fcd16;
                            src += Character.CharCount(c);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                // copy these code units all at once
                if (src != prevSrc)
                {
                    if (src == limit)
                    {
                        buffer.Append(s.Slice(prevSrc, src - prevSrc)); // ICU4N: Corrected 3rd parameter
                        break;
                    }
                    prevBoundary = src;
                    // We know that the previous character's lccc==0.
                    if (prevFCD16 < 0)
                    {
                        // Fetching the fcd16 value was deferred for this below-minLcccCP code point.
                        int prev = ~prevFCD16;
                        if (prev < minDecompNoCP)
                        {
                            prevFCD16 = 0;
                        }
                        else
                        {
                            prevFCD16 = GetFCD16FromNormData(prev);
                            if (prevFCD16 > 1)
                            {
                                --prevBoundary;
                            }
                        }
                    }
                    else
                    {
                        int p = src - 1;
                        if (char.IsLowSurrogate(s[p]) && prevSrc < p &&
                            char.IsHighSurrogate(s[p - 1])
                        )
                        {
                            --p;
                            // Need to fetch the previous character's FCD value because
                            // prevFCD16 was just for the trail surrogate code point.
                            prevFCD16 = GetFCD16FromNormData(Character.ToCodePoint(s[p], s[p + 1]));
                            // Still known to have lccc==0 because its lead surrogate unit had lccc==0.
                        }
                        if (prevFCD16 > 1)
                        {
                            prevBoundary = p;
                        }
                    }
                    // The last lccc==0 character is excluded from the
                    // flush-and-append call in case it needs to be modified.
                    // ICU4N: Call Append() rather than FlushAndAppendZeroCC() on ReorderingBuffer
                    buffer.Append(s.Slice(prevSrc, prevBoundary - prevSrc)); // ICU4N: Corrected 3rd parameter
                    buffer.Append(s.Slice(prevBoundary, src - prevBoundary)); // ICU4N: Corrected 3rd parameter
                    // The start of the current character (c).
                    prevSrc = src;
                }
                else if (src == limit)
                {
                    break;
                }

                src += Character.CharCount(c);
                // The current character (c) at [prevSrc..src[ has a non-zero lead combining class.
                // Check for proper order, and decompose locally if necessary.
                if ((prevFCD16 & 0xff) <= (fcd16 >> 8))
                {
                    // proper order: prev tccc <= current lccc
                    if ((fcd16 & 0xff) <= 1)
                    {
                        prevBoundary = src;
                    }
                    buffer.AppendZeroCC(c);
                    prevFCD16 = fcd16;
                    continue;
                }
                else
                {
                    /*
                     * Back out the part of the source that we copied or appended
                     * already but is now going to be decomposed.
                     * prevSrc is set to after what was copied/appended.
                     */
                    buffer.RemoveSuffix(prevSrc - prevBoundary);
                    /*
                     * Find the part of the source that needs to be decomposed,
                     * up to the next safe boundary.
                     */
                    src = FindNextFCDBoundary(s, src, limit);
                    /*
                     * The source text does not fulfill the conditions for FCD.
                     * Decompose and reorder a limited piece of the text.
                     */
                    DecomposeShort(s, prevBoundary, src, false, false, ref buffer);
                    prevBoundary = src;
                    prevFCD16 = 0;
                }
            }
            return src;
        }

        // normalize
        // ICU4N: Separated dual functionality that was in ICU4J into MakeFCD() and MakeFCDSpanQuickCheckYes()
        public int MakeFCDQuickCheck(ReadOnlySpan<char> s)
        {
            // Note: In this function we use buffer->appendZeroCC() because we track
            // the lead and trail combining classes here, rather than leaving it to
            // the ReorderingBuffer.
            // The exception is the call to decomposeShort() which uses the buffer
            // in the normal way.

            int src = 0, limit = s.Length;

            // Tracks the last FCD-safe boundary, before lccc=0 or after properly-ordered tccc<=1.
            // Similar to the prevBoundary in the compose() implementation.
            int prevBoundary = src;
            int prevSrc;
            int c = 0;
            int prevFCD16 = 0;
            int fcd16 = 0;

            for (; ; )
            {
                // count code units with lccc==0
                for (prevSrc = src; src != limit;)
                {
                    if ((c = s[src]) < minLcccCP)
                    {
                        prevFCD16 = ~c;
                        ++src;
                    }
                    else if (!SingleLeadMightHaveNonZeroFCD16(c))
                    {
                        prevFCD16 = 0;
                        ++src;
                    }
                    else
                    {
                        if (UTF16.IsSurrogate((char)c))
                        {
                            char c2;
                            if (UTF16Plus.IsSurrogateLead(c))
                            {
                                if ((src + 1) != limit && char.IsLowSurrogate(c2 = s[src + 1]))
                                {
                                    c = Character.ToCodePoint((char)c, c2);
                                }
                            }
                            else /* trail surrogate */
                            {
                                if (prevSrc < src && char.IsHighSurrogate(c2 = s[src - 1]))
                                {
                                    --src;
                                    c = Character.ToCodePoint(c2, (char)c);
                                }
                            }
                        }
                        if ((fcd16 = GetFCD16FromNormData(c)) <= 0xff)
                        {
                            prevFCD16 = fcd16;
                            src += Character.CharCount(c);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                // copy these code units all at once
                if (src != prevSrc)
                {
                    if (src == limit)
                    {
                        break;
                    }
                    prevBoundary = src;
                    // We know that the previous character's lccc==0.
                    if (prevFCD16 < 0)
                    {
                        // Fetching the fcd16 value was deferred for this below-minLcccCP code point.
                        int prev = ~prevFCD16;
                        if (prev < minDecompNoCP)
                        {
                            prevFCD16 = 0;
                        }
                        else
                        {
                            prevFCD16 = GetFCD16FromNormData(prev);
                            if (prevFCD16 > 1)
                            {
                                --prevBoundary;
                            }
                        }
                    }
                    else
                    {
                        int p = src - 1;
                        if (char.IsLowSurrogate(s[p]) && prevSrc < p &&
                            char.IsHighSurrogate(s[p - 1])
                        )
                        {
                            --p;
                            // Need to fetch the previous character's FCD value because
                            // prevFCD16 was just for the trail surrogate code point.
                            prevFCD16 = GetFCD16FromNormData(Character.ToCodePoint(s[p], s[p + 1]));
                            // Still known to have lccc==0 because its lead surrogate unit had lccc==0.
                        }
                        if (prevFCD16 > 1)
                        {
                            prevBoundary = p;
                        }
                    }
                    // The last lccc==0 character is excluded from the
                    // flush-and-append call in case it needs to be modified.
                    // The start of the current character (c).
                    prevSrc = src;
                }
                else if (src == limit)
                {
                    break;
                }

                src += Character.CharCount(c);
                // The current character (c) at [prevSrc..src[ has a non-zero lead combining class.
                // Check for proper order, and decompose locally if necessary.
                if ((prevFCD16 & 0xff) <= (fcd16 >> 8))
                {
                    // proper order: prev tccc <= current lccc
                    if ((fcd16 & 0xff) <= 1)
                    {
                        prevBoundary = src;
                    }
                    prevFCD16 = fcd16;
                    continue;
                }
                else
                {
                    return prevBoundary;  // quick check "no"
                }
            }
            return src;
        }

        public void MakeFCDAndAppend(ReadOnlySpan<char> s, bool doMakeFCD, ref ReorderingBuffer buffer)
        {
            int src = 0, limit = s.Length;
            if (!buffer.IsEmpty)
            {
                int firstBoundaryInSrc = FindNextFCDBoundary(s, 0, limit);
                if (0 != firstBoundaryInSrc)
                {
                    int lastBoundaryInDest = FindPreviousFCDBoundary(buffer.AsSpan(),
                                                                   buffer.Length);
                    int middleLength = (buffer.Length - lastBoundaryInDest) + firstBoundaryInSrc + 16;
                    ValueStringBuilder middle = middleLength <= CharStackBufferSize
                        ? new ValueStringBuilder(stackalloc char[middleLength])
                        : new ValueStringBuilder(middleLength);
                    try
                    {
                        middle.Append(buffer.AsSpan(lastBoundaryInDest, buffer.Length - lastBoundaryInDest)); // ICU4N : Fixed length parameter
                        buffer.RemoveSuffix(buffer.Length - lastBoundaryInDest);
                        middle.Append(s.Slice(0, firstBoundaryInSrc - 0));
                        MakeFCD(middle.AsSpan(), ref buffer);
                        src = firstBoundaryInSrc;
                    }
                    finally
                    {
                        middle.Dispose();
                    }
                }
            }
            if (doMakeFCD)
            {
                MakeFCD(s.Slice(src, limit - src), ref buffer); // ICU4N: Corrected length parameter
            }
            else
            {
                buffer.Append(s.Slice(src, limit - src)); // ICU4N: Corrected 3rd parameter
            }
        }

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
                return norm16 <= MinNormalMaybeYes || norm16 == JamoVt;
            }
            // c decomposes, get everything from the variable-length extra data
            int mapping = norm16 >> OffsetShift;
            int firstUnit = extraData[mapping];
            // true if leadCC==0 (hasFCDBoundaryBefore())
            return (firstUnit & MappingHasCccLcccWord) == 0 || (extraData[mapping - 1] & 0xff00) == 0;
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
                    return norm16 <= MinNormalMaybeYes || norm16 == JamoVt;
                }
                // Maps to an isCompYesAndZeroCC.
                return (norm16 & DeltaTcccMask) <= DeltaTccc1;
            }
            // c decomposes, get everything from the variable-length extra data
            int mapping = norm16 >> OffsetShift;
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
            return (firstUnit & MappingHasCccLcccWord) == 0 || (extraData[mapping - 1] & 0xff00) == 0;
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
                (norm16 & HasCompBoundaryAfterValue) != 0 &&
                (!onlyContiguous || IsInert(norm16) || extraData[norm16 >> OffsetShift] <= 0x1ff);
        }

        public bool HasFCDBoundaryBefore(int c) { return HasDecompBoundaryBefore(c); }
        public bool HasFCDBoundaryAfter(int c) { return HasDecompBoundaryAfter(c); }
        public bool IsFCDInert(int c) { return GetFCD16(c) <= 1; }

        private bool IsMaybe(int norm16) { return minMaybeYes <= norm16 && norm16 <= JamoVt; }
        private bool IsMaybeOrNonZeroCC(int norm16) { return norm16 >= minMaybeYes; }
        private static bool IsInert(int norm16) { return norm16 == Inert; }
        private static bool IsJamoL(int norm16) { return norm16 == JamoL; }
        private static bool IsJamoVT(int norm16) { return norm16 == JamoVt; }
        private int HangulLVT() { return minYesNoMappingsOnly | HasCompBoundaryAfterValue; }
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
                   norm16 == JamoVt ||
                   (minMaybeYes <= norm16 && norm16 <= MinNormalMaybeYes);
        }
        /// <summary>
        /// A little faster and simpler than <see cref="IsDecompYesAndZeroCC(int)"/> but does not include
        /// the MaybeYes which combine-forward and have ccc=0.
        /// (Standard Unicode 10 normalization does not have such characters.)
        /// </summary>
        private bool IsMostDecompYesAndZeroCC(int norm16)
        {
            return norm16 < minYesNo || norm16 == MinNormalMaybeYes || norm16 == JamoVt;
        }
        private bool IsDecompNoAlgorithmic(int norm16) { return norm16 >= limitNoNo; }

        // For use with isCompYes().
        // Perhaps the compiler can combine the two tests for MIN_YES_YES_WITH_CC.
        // static uint8_t getCCFromYes(uint16_t norm16) {
        //     return norm16>=MIN_YES_YES_WITH_CC ? getCCFromNormalYesOrMaybe(norm16) : 0;
        // }
        private int GetCCFromNoNo(int norm16)
        {
            int mapping = norm16 >> OffsetShift;
            if ((extraData[mapping] & MappingHasCccLcccWord) != 0)
            {
                return extraData[mapping - 1] & 0xff;
            }
            else
            {
                return 0;
            }
        }
        private int GetTrailCCFromCompYesAndZeroCC(int norm16)
        {
            if (norm16 <= minYesNo)
            {
                return 0;  // yesYes and Hangul LV have ccc=tccc=0
            }
            else
            {
                // For Hangul LVT we harmlessly fetch a firstUnit with tccc=0 here.
                return extraData[norm16 >> OffsetShift] >> 8;  // tccc from yesNo
            }
        }

        // Requires algorithmic-NoNo.
        private int MapAlgorithmic(int c, int norm16)
        {
            return c + (norm16 >> DeltaShift) - centerNoNoDelta;
        }

        // Requires minYesNo<norm16<limitNoNo.
        // private int getMapping(int norm16) { return extraData+(norm16>>OFFSET_SHIFT); }

        /// <returns>Index into maybeYesCompositions, or -1.</returns>
        private int GetCompositionsListForDecompYes(int norm16)
        {
            if (norm16 < JamoL || MinNormalMaybeYes <= norm16)
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
                    norm16 += MinNormalMaybeYes;  // for yesYes; if Jamo L: harmless empty list
                }
                return norm16 >> OffsetShift;
            }
        }
        /// <returns>Index into maybeYesCompositions.</returns>
        private int GetCompositionsListForComposite(int norm16)
        {
            // A composite has both mapping & compositions list.
            int list = ((MinNormalMaybeYes - minMaybeYes) + norm16) >> OffsetShift;
            int firstUnit = maybeYesCompositions[list];
            return list +  // mapping in maybeYesCompositions
                1 +  // +1 to skip the first unit with the mapping length
                (firstUnit & MappingLengthMask);  // + mapping length
        }
        private int GetCompositionsListForMaybe(int norm16)
        {
            // minMaybeYes<=norm16<MIN_NORMAL_MAYBE_YES
            return (norm16 - minMaybeYes) >> OffsetShift;
        }

        /// <param name="norm16">Code point must have compositions.</param>
        /// <returns>Index into maybeYesCompositions.</returns>
        private int GetCompositionsList(int norm16)
        {
            return IsDecompYes(norm16) ?
                    GetCompositionsListForDecompYes(norm16) :
                    GetCompositionsListForComposite(norm16);
        }

        // Decompose a short piece of text which is likely to contain characters that
        // fail the quick check loop and/or where the quick check loop's overhead
        // is unlikely to be amortized.
        // Called by the Compose() and MakeFCD() implementations.
        // Public in .NET for collation implementation code.
        private int DecomposeShort(
                scoped ReadOnlySpan<char> s, int src, int limit,
                bool stopAtCompBoundary, bool onlyContiguous,
                scoped ref ReorderingBuffer buffer)
        {
            while (src < limit)
            {
                int c = Character.CodePointAt(s, src);
                if (stopAtCompBoundary && c < minCompNoMaybeCP)
                {
                    return src;
                }
                int norm16 = GetNorm16(c);
                if (stopAtCompBoundary && Norm16HasCompBoundaryBefore(norm16))
                {
                    return src;
                }
                src += Character.CharCount(c);
                Decompose(c, norm16, ref buffer);
                if (stopAtCompBoundary && Norm16HasCompBoundaryAfter(norm16, onlyContiguous))
                {
                    return src;
                }
            }
            return src;
        }

        private void Decompose(int c, int norm16, scoped ref ReorderingBuffer buffer)
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
                buffer.AppendHangulDecomposition(c);
            }
            else
            {
                // c decomposes, get everything from the variable-length extra data
                int mapping = norm16 >> OffsetShift;
                int firstUnit = extraData[mapping];
                int length = firstUnit & MappingLengthMask;
                int leadCC, trailCC;
                trailCC = firstUnit >> 8;
                if ((firstUnit & MappingHasCccLcccWord) != 0)
                {
                    leadCC = extraData[mapping - 1] >> 8;
                }
                else
                {
                    leadCC = 0;
                }
                ++mapping;  // skip over the firstUnit
                buffer.Append(extraData, mapping, length, leadCC, trailCC); // ICU4N: Corrected 3rd parameter
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
            if (trail < Comp1TrailLimit)
            {
                // trail character is 0..33FF
                // result entry may have 2 or 3 units
                key1 = (trail << 1);
                while (key1 > (firstUnit = compositions[list]))
                {
                    list += 2 + (firstUnit & Comp1Triple);
                }
                if (key1 == (firstUnit & Comp1TrailMask))
                {
                    if ((firstUnit & Comp1Triple) != 0)
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
                key1 = Comp1TrailLimit + (((trail >> Comp1TrailShift)) & ~Comp1Triple);
                int key2 = (trail << Comp2TrailShift) & 0xffff;
                int secondUnit;
                for (; ; )
                {
                    if (key1 > (firstUnit = compositions[list]))
                    {
                        list += 2 + (firstUnit & Comp1Triple);
                    }
                    else if (key1 == (firstUnit & Comp1TrailMask))
                    {
                        if (key2 > (secondUnit = compositions[list + 1]))
                        {
                            if ((firstUnit & Comp1LastTuple) != 0)
                            {
                                break;
                            }
                            else
                            {
                                list += 3;
                            }
                        }
                        else if (key2 == (secondUnit & Comp2TrailMask))
                        {
                            return ((secondUnit & ~Comp2TrailMask) << 16) | compositions[list + 2];
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
                if ((firstUnit & Comp1Triple) == 0)
                {
                    compositeAndFwd = maybeYesCompositions[list + 1];
                    list += 2;
                }
                else
                {
                    compositeAndFwd = ((maybeYesCompositions[list + 1] & ~Comp2TrailMask) << 16) |
                                    maybeYesCompositions[list + 2];
                    list += 3;
                }
                int composite = compositeAndFwd >> 1;
                if ((compositeAndFwd & 1) != 0)
                {
                    AddComposites(GetCompositionsListForComposite(GetNorm16(composite)), set);
                }
                set.Add(composite);
            } while ((firstUnit & Comp1LastTuple) == 0);
        }

        /// <summary>
        /// Recomposes the buffer text starting at <paramref name="startIndex"/>
        /// (which is in NFD - decomposed and canonically ordered),
        /// and truncates the buffer contents.
        /// </summary>
        /// <remarks>
        /// Note that recomposition never lengthens the text:
        /// Any character consists of either one or two code units;
        /// a composition may contain at most one more code unit than the original starter,
        /// while the combining mark that is removed has at least one code unit.
        /// </remarks>
        private void Recompose(scoped ref ReorderingBuffer buffer, int startIndex,
                               bool onlyContiguous)
        {
            ref ReorderingBuffer sb = ref buffer;
            int p = startIndex;
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
                                sb.Remove(pRemove, p - pRemove); // ICU4N: Corrected 2nd parameter
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
                        sb.Remove(pRemove, p - pRemove); // ICU4N: Corrected 2nd parameter
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
                    list = ((MinNormalMaybeYes - minMaybeYes) + norm16) >> OffsetShift;
                    if (norm16 > minYesNo)
                    {  // composite 'a' has both mapping & compositions list
                        list +=  // mapping pointer
                            1 +  // +1 to skip the first unit with the mapping length
                            (maybeYesCompositions[list] & MappingLengthMask);  // + mapping length
                    }
                }
            }
            else if (norm16 < minMaybeYes || MinNormalMaybeYes <= norm16)
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

        private bool HasCompBoundaryBefore(ReadOnlySpan<char> s, int src, int limit)
        {
            return src == limit || HasCompBoundaryBefore(Character.CodePointAt(s, src));
        }

        private bool Norm16HasCompBoundaryAfter(int norm16, bool onlyContiguous)
        {
            return (norm16 & HasCompBoundaryAfterValue) != 0 &&
                (!onlyContiguous || IsTrailCC01ForCompBoundaryAfter(norm16));
        }

        private bool HasCompBoundaryAfter(ReadOnlySpan<char> s, int start, int p, bool onlyContiguous)
        {
            return start == p || HasCompBoundaryAfter(Character.CodePointBefore(s, p), onlyContiguous);
        }

        /// <summary>For FCC: Given norm16 HAS_COMP_BOUNDARY_AFTER, does it have tccc&lt;=1?</summary>
        private bool IsTrailCC01ForCompBoundaryAfter(int norm16)
        {
            return IsInert(norm16) || (IsDecompNoAlgorithmic(norm16) ?
                (norm16 & DeltaTcccMask) <= DeltaTccc1 : extraData[norm16 >> OffsetShift] <= 0x1ff);
        }

        private int FindPreviousCompBoundary(ReadOnlySpan<char> s, int p, bool onlyContiguous)
        {
            while (p > 0)
            {
                int c = Character.CodePointBefore(s, p);
                int norm16 = GetNorm16(c);
                if (Norm16HasCompBoundaryAfter(norm16, onlyContiguous))
                {
                    break;
                }
                p -= Character.CharCount(c);
                if (HasCompBoundaryBefore(c, norm16))
                {
                    break;
                }
            }
            return p;
        }

        private int FindNextCompBoundary(ReadOnlySpan<char> s, int p, int limit, bool onlyContiguous)
        {
            while (p < limit)
            {
                int c = Character.CodePointAt(s, p);
                int norm16 = normTrie.Get(c);
                if (HasCompBoundaryBefore(c, norm16))
                {
                    break;
                }
                p += Character.CharCount(c);
                if (Norm16HasCompBoundaryAfter(norm16, onlyContiguous))
                {
                    break;
                }
            }
            return p;
        }

        private int FindPreviousFCDBoundary(ReadOnlySpan<char> s, int p)
        {
            while (p > 0)
            {
                int c = Character.CodePointBefore(s, p);
                int norm16;
                if (c < minDecompNoCP || Norm16HasDecompBoundaryAfter(norm16 = GetNorm16(c)))
                {
                    break;
                }
                p -= Character.CharCount(c);
                if (Norm16HasDecompBoundaryBefore(norm16))
                {
                    break;
                }
            }
            return p;
        }

        private int FindNextFCDBoundary(ReadOnlySpan<char> s, int p, int limit)
        {
            while (p < limit)
            {
                int c = Character.CodePointAt(s, p);
                int norm16;
                if (c < minLcccCP || Norm16HasDecompBoundaryBefore(norm16 = GetNorm16(c)))
                {
                    break;
                }
                p += Character.CharCount(c);
                if (Norm16HasDecompBoundaryAfter(norm16))
                {
                    break;
                }
            }
            return p;
        }

        private int GetPreviousTrailCC(ReadOnlySpan<char> s, int start, int p)
        {
            if (start == p)
            {
                return 0;
            }
            return GetFCD16(Character.CodePointBefore(s, p));
        }

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

        private VersionInfo? dataVersion;

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
