using ICU4N.Support;
using ICU4N.Support.IO;
using ICU4N.Support.Text;
using ICU4N.Text;
using ICU4N.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Hangul = ICU4N.Impl.Normalizer2Impl.Hangul;
using ReorderingBuffer = ICU4N.Impl.Normalizer2Impl.ReorderingBuffer;
using UTF16Plus = ICU4N.Impl.Normalizer2Impl.UTF16Plus;

namespace ICU4N.Impl
{
    /// <summary>
    /// Low-level implementation of the Unicode Normalization Algorithm.
    /// For the data structure and details see the documentation at the end of
    /// C++ normalizer2impl.h and in the design doc at
    /// http://site.icu-project.org/design/normalization/custom
    /// </summary>
    public sealed class Normalizer2Impl
    {
        public sealed class Hangul
        {
            /* Korean Hangul and Jamo constants */
            public static readonly int JAMO_L_BASE = 0x1100;     /* "lead" jamo */
            public static readonly int JAMO_L_END = 0x1112;
            public static readonly int JAMO_V_BASE = 0x1161;     /* "vowel" jamo */
            public static readonly int JAMO_V_END = 0x1175;
            public static readonly int JAMO_T_BASE = 0x11a7;     /* "trail" jamo */
            public static readonly int JAMO_T_END = 0x11c2;

            public static readonly int HANGUL_BASE = 0xac00;
            public static readonly int HANGUL_END = 0xd7a3;

            public static readonly int JAMO_L_COUNT = 19;
            public static readonly int JAMO_V_COUNT = 21;
            public static readonly int JAMO_T_COUNT = 28;

            public static readonly int JAMO_L_LIMIT = JAMO_L_BASE + JAMO_L_COUNT;
            public static readonly int JAMO_V_LIMIT = JAMO_V_BASE + JAMO_V_COUNT;

            public static readonly int JAMO_VT_COUNT = JAMO_V_COUNT * JAMO_T_COUNT;

            public static readonly int HANGUL_COUNT = JAMO_L_COUNT * JAMO_V_COUNT * JAMO_T_COUNT;
            public static readonly int HANGUL_LIMIT = HANGUL_BASE + HANGUL_COUNT;

            public static bool IsHangul(int c)
            {
                return HANGUL_BASE <= c && c < HANGUL_LIMIT;
            }
            public static bool IsHangulLV(int c)
            {
                c -= HANGUL_BASE;
                return 0 <= c && c < HANGUL_COUNT && c % JAMO_T_COUNT == 0;
            }
            public static bool IsJamoL(int c)
            {
                return JAMO_L_BASE <= c && c < JAMO_L_LIMIT;
            }
            public static bool IsJamoV(int c)
            {
                return JAMO_V_BASE <= c && c < JAMO_V_LIMIT;
            }
            public static bool IsJamoT(int c)
            {
                int t = c - JAMO_T_BASE;
                return 0 < t && t < JAMO_T_COUNT;  // not JAMO_T_BASE itself
            }
            public static bool IsJamo(int c)
            {
                return JAMO_L_BASE <= c && c <= JAMO_T_END &&
                    (c <= JAMO_L_END || (JAMO_V_BASE <= c && c <= JAMO_V_END) || JAMO_T_BASE < c);
            }

            /// <summary>
            /// Decomposes <paramref name="c"/>, which must be a Hangul syllable, into buffer
            /// and returns the length of the decomposition (2 or 3).
            /// </summary>
            public static int Decompose(int c, StringBuilder buffer)
            {
                try
                {
                    c -= HANGUL_BASE;
                    int c2 = c % JAMO_T_COUNT;
                    c /= JAMO_T_COUNT;
                    buffer.Append((char)(JAMO_L_BASE + c / JAMO_V_COUNT));
                    buffer.Append((char)(JAMO_V_BASE + c % JAMO_V_COUNT));
                    if (c2 == 0)
                    {
                        return 2;
                    }
                    else
                    {
                        buffer.Append((char)(JAMO_T_BASE + c2));
                        return 3;
                    }
                }
                catch (IOException e)
                {
                    // Will not occur because we do not write to I/O.
                    throw new ICUUncheckedIOException(e);
                }
            }

            /// <summary>
            /// Decomposes <paramref name="c"/>, which must be a Hangul syllable, into buffer
            /// and returns the length of the decomposition (2 or 3).
            /// </summary>
            internal static int Decompose(int c, ReorderingBuffer buffer)
            {
                try
                {
                    c -= HANGUL_BASE;
                    int c2 = c % JAMO_T_COUNT;
                    c /= JAMO_T_COUNT;
                    buffer.Append((char)(JAMO_L_BASE + c / JAMO_V_COUNT));
                    buffer.Append((char)(JAMO_V_BASE + c % JAMO_V_COUNT));
                    if (c2 == 0)
                    {
                        return 2;
                    }
                    else
                    {
                        buffer.Append((char)(JAMO_T_BASE + c2));
                        return 3;
                    }
                }
                catch (IOException e)
                {
                    // Will not occur because we do not write to I/O.
                    throw new ICUUncheckedIOException(e);
                }
            }

            /// <summary>
            /// Decomposes <paramref name="c"/>, which must be a Hangul syllable, into buffer.
            /// This is the raw, not recursive, decomposition. Its length is always 2.
            /// </summary>
            public static void GetRawDecomposition(int c, StringBuilder buffer)
            {
                try
                {
                    int orig = c;
                    c -= HANGUL_BASE;
                    int c2 = c % JAMO_T_COUNT;
                    if (c2 == 0)
                    {
                        c /= JAMO_T_COUNT;
                        buffer.Append((char)(JAMO_L_BASE + c / JAMO_V_COUNT));
                        buffer.Append((char)(JAMO_V_BASE + c % JAMO_V_COUNT));
                    }
                    else
                    {
                        buffer.Append((char)(orig - c2));  // LV syllable
                        buffer.Append((char)(JAMO_T_BASE + c2));
                    }
                }
                catch (IOException e)
                {
                    // Will not occur because we do not write to I/O.
                    throw new ICUUncheckedIOException(e);
                }
            }

            /// <summary>
            /// Decomposes <paramref name="c"/>, which must be a Hangul syllable, into buffer.
            /// This is the raw, not recursive, decomposition. Its length is always 2.
            /// </summary>
            internal static void GetRawDecomposition(int c, ReorderingBuffer buffer)
            {
                try
                {
                    int orig = c;
                    c -= HANGUL_BASE;
                    int c2 = c % JAMO_T_COUNT;
                    if (c2 == 0)
                    {
                        c /= JAMO_T_COUNT;
                        buffer.Append((char)(JAMO_L_BASE + c / JAMO_V_COUNT));
                        buffer.Append((char)(JAMO_V_BASE + c % JAMO_V_COUNT));
                    }
                    else
                    {
                        buffer.Append((char)(orig - c2));  // LV syllable
                        buffer.Append((char)(JAMO_T_BASE + c2));
                    }
                }
                catch (IOException e)
                {
                    // Will not occur because we do not write to I/O.
                    throw new ICUUncheckedIOException(e);
                }
            }
        }

        /**
         * Writable buffer that takes care of canonical ordering.
         * Its Appendable methods behave like the C++ implementation's
         * appendZeroCC() methods.
         * <p>
         * If dest is a StringBuilder, then the buffer writes directly to it.
         * Otherwise, the buffer maintains a StringBuilder for intermediate text segments
         * until no further changes are necessary and whole segments are appended.
         * append() methods that take combining-class values always write to the StringBuilder.
         * Other append() methods flush and append to the Appendable.
         */
        public sealed class ReorderingBuffer : IAppendable
        {
            public ReorderingBuffer(Normalizer2Impl ni, StringBuilder dest, int destCapacity)
                : this(ni, dest.ToAppendable(), destCapacity)
            {
            }

            internal ReorderingBuffer(Normalizer2Impl ni, IAppendable dest, int destCapacity)
            {
                impl = ni;
                app = dest;
                if (app is StringBuilderAppendable)
                {
                    appIsStringBuilder = true;
                    str = ((StringBuilderAppendable)dest).StringBuilder;
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

            public bool Equals(string s, int start, int limit)
            {
                return UTF16Plus.Equal(str, 0, str.Length, s, start, limit);
            }

            public bool Equals(StringBuilder s, int start, int limit)
            {
                return UTF16Plus.Equal(str, 0, str.Length, s, start, limit);
            }

            public bool Equals(char[] s, int start, int limit)
            {
                return UTF16Plus.Equal(str, 0, str.Length, s, start, limit);
            }

            internal bool Equals(ICharSequence s, int start, int limit)
            {
                return UTF16Plus.Equal(str.ToCharSequence(), 0, str.Length, s, start, limit);
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
            public void Append(string s, int start, int limit,
                               int leadCC, int trailCC)
            {
                Append(new StringCharSequence(s), start, limit, leadCC, trailCC);
            }

            // s must be in NFD, otherwise change the implementation.
            public void Append(StringBuilder s, int start, int limit,
                           int leadCC, int trailCC)
            {
                Append(new StringBuilderCharSequence(s), start, limit, leadCC, trailCC);
            }

            // s must be in NFD, otherwise change the implementation.
            public void Append(char[] s, int start, int limit,
                               int leadCC, int trailCC)
            {
                Append(new CharArrayCharSequence(s), start, limit, leadCC, trailCC);
            }

            // s must be in NFD, otherwise change the implementation.
            internal void Append(ICharSequence s, int start, int limit,
                           int leadCC, int trailCC)
            {
                if (start == limit)
                {
                    return;
                }
                if (lastCC <= leadCC || leadCC == 0)
                {
                    if (trailCC <= 1)
                    {
                        reorderStart = str.Length + (limit - start);
                    }
                    else if (leadCC <= 1)
                    {
                        reorderStart = str.Length + 1;  // Ok if not a code point boundary.
                    }
                    str.Append(s, start, limit);
                    lastCC = trailCC;
                }
                else
                {
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
                            leadCC = GetCCFromYesOrMaybe(impl.GetNorm16(c));
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

            public ReorderingBuffer Append(string s)
            {
                return Append(new StringCharSequence(s));
            }

            public ReorderingBuffer Append(StringBuilder s)
            {
                return Append(new StringBuilderCharSequence(s));
            }

            public ReorderingBuffer Append(char[] s)
            {
                return Append(new CharArrayCharSequence(s));
            }

            internal ReorderingBuffer Append(ICharSequence s)
            {
                if (s.Length != 0)
                {
                    str.Append(s);
                    lastCC = 0;
                    reorderStart = str.Length;
                }
                return this;
            }

            public ReorderingBuffer Append(string s, int start, int limit)
            {
                return Append(new StringCharSequence(s), start, limit);
            }

            public ReorderingBuffer Append(StringBuilder s, int start, int limit)
            {
                return Append(new StringBuilderCharSequence(s), start, limit);
            }

            public ReorderingBuffer Append(char[] s, int start, int limit)
            {
                return Append(new CharArrayCharSequence(s), start, limit);
            }

            internal ReorderingBuffer Append(ICharSequence s, int start, int limit)
            {
                if (start != limit)
                {
                    str.Append(s, start, limit);
                    lastCC = 0;
                    reorderStart = str.Length;
                }
                return this;
            }
            /**
             * Flushes from the intermediate StringBuilder to the Appendable,
             * if they are different objects.
             * Used after recomposition.
             * Must be called at the end when writing to a non-StringBuilder Appendable.
             */
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

            /// <summary>
            /// Flushes from the intermediate StringBuilder to the IAppendable,
            /// if they are different objects.
            /// Then appends the new text to the IAppendable or StringBuilder.
            /// Normally used after quick check loops find a non-empty sequence.
            /// </summary>
            public ReorderingBuffer FlushAndAppendZeroCC(string s, int start, int limit)
            {
                return FlushAndAppendZeroCC(new StringCharSequence(s), start, limit);
            }

            /// <summary>
            /// Flushes from the intermediate StringBuilder to the IAppendable,
            /// if they are different objects.
            /// Then appends the new text to the IAppendable or StringBuilder.
            /// Normally used after quick check loops find a non-empty sequence.
            /// </summary>
            public ReorderingBuffer FlushAndAppendZeroCC(StringBuilder s, int start, int limit)
            {
                return FlushAndAppendZeroCC(new StringBuilderCharSequence(s), start, limit);
            }

            /// <summary>
            /// Flushes from the intermediate StringBuilder to the IAppendable,
            /// if they are different objects.
            /// Then appends the new text to the IAppendable or StringBuilder.
            /// Normally used after quick check loops find a non-empty sequence.
            /// </summary>
            public ReorderingBuffer FlushAndAppendZeroCC(char[] s, int start, int limit)
            {
                return FlushAndAppendZeroCC(new CharArrayCharSequence(s), start, limit);
            }

            /// <summary>
            /// Flushes from the intermediate StringBuilder to the IAppendable,
            /// if they are different objects.
            /// Then appends the new text to the IAppendable or StringBuilder.
            /// Normally used after quick check loops find a non-empty sequence.
            /// </summary>
            internal ReorderingBuffer FlushAndAppendZeroCC(ICharSequence s, int start, int limit)
            {
                if (appIsStringBuilder)
                {
                    str.Append(s, start, limit);
                    reorderStart = str.Length;
                }
                else
                {
                    try
                    {
                        app.Append(str).Append(s, start, limit);
                        str.Length = 0;
                        reorderStart = 0;
                    }
                    catch (IOException e)
                    {
                        throw new ICUUncheckedIOException(e);  // Avoid declaring "throws IOException".
                    }
                }
                lastCC = 0;
                return this;
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
                str.Delete(oldLength - suffixLength, oldLength);
                lastCC = 0;
                reorderStart = str.Length;
            }

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
        public sealed class UTF16Plus
        {
            /**
             * Assuming c is a surrogate code point (UTF16.isSurrogate(c)),
             * is it a lead surrogate?
             * @param c code unit or code point
             * @return true or false
             */
            public static bool IsSurrogateLead(int c) { return (c & 0x400) == 0; }

            /// <summary>
            /// Compares two ICharSequence objects for binary equality.
            /// </summary>
            /// <param name="s1">s1 first sequence</param>
            /// <param name="s2">s2 second sequence</param>
            /// <returns>true if s1 contains the same text as s2.</returns>
            public static bool Equal(string s1, string s2)
            {
                return Equal(s1.ToCharSequence(), s2.ToCharSequence());
            }

            /// <summary>
            /// Compares two ICharSequence objects for binary equality.
            /// </summary>
            /// <param name="s1">s1 first sequence</param>
            /// <param name="s2">s2 second sequence</param>
            /// <returns>true if s1 contains the same text as s2.</returns>
            public static bool Equal(string s1, StringBuilder s2)
            {
                return Equal(s1.ToCharSequence(), s2.ToCharSequence());
            }

            /// <summary>
            /// Compares two ICharSequence objects for binary equality.
            /// </summary>
            /// <param name="s1">s1 first sequence</param>
            /// <param name="s2">s2 second sequence</param>
            /// <returns>true if s1 contains the same text as s2.</returns>
            public static bool Equal(string s1, char[] s2)
            {
                return Equal(s1.ToCharSequence(), s2.ToCharSequence());
            }

            /// <summary>
            /// Compares two ICharSequence objects for binary equality.
            /// </summary>
            /// <param name="s1">s1 first sequence</param>
            /// <param name="s2">s2 second sequence</param>
            /// <returns>true if s1 contains the same text as s2.</returns>
            public static bool Equal(StringBuilder s1, string s2)
            {
                return Equal(s1.ToCharSequence(), s2.ToCharSequence());
            }

            /// <summary>
            /// Compares two ICharSequence objects for binary equality.
            /// </summary>
            /// <param name="s1">s1 first sequence</param>
            /// <param name="s2">s2 second sequence</param>
            /// <returns>true if s1 contains the same text as s2.</returns>
            public static bool Equal(StringBuilder s1, StringBuilder s2)
            {
                return Equal(s1.ToCharSequence(), s2.ToCharSequence());
            }

            /// <summary>
            /// Compares two ICharSequence objects for binary equality.
            /// </summary>
            /// <param name="s1">s1 first sequence</param>
            /// <param name="s2">s2 second sequence</param>
            /// <returns>true if s1 contains the same text as s2.</returns>
            public static bool Equal(StringBuilder s1, char[] s2)
            {
                return Equal(s1.ToCharSequence(), s2.ToCharSequence());
            }

            /// <summary>
            /// Compares two ICharSequence objects for binary equality.
            /// </summary>
            /// <param name="s1">s1 first sequence</param>
            /// <param name="s2">s2 second sequence</param>
            /// <returns>true if s1 contains the same text as s2.</returns>
            public static bool Equal(char[] s1, string s2)
            {
                return Equal(s1.ToCharSequence(), s2.ToCharSequence());
            }

            /// <summary>
            /// Compares two ICharSequence objects for binary equality.
            /// </summary>
            /// <param name="s1">s1 first sequence</param>
            /// <param name="s2">s2 second sequence</param>
            /// <returns>true if s1 contains the same text as s2.</returns>
            public static bool Equal(char[] s1, StringBuilder s2)
            {
                return Equal(s1.ToCharSequence(), s2.ToCharSequence());
            }

            /// <summary>
            /// Compares two ICharSequence objects for binary equality.
            /// </summary>
            /// <param name="s1">s1 first sequence</param>
            /// <param name="s2">s2 second sequence</param>
            /// <returns>true if s1 contains the same text as s2.</returns>
            public static bool Equal(char[] s1, char[] s2)
            {
                return Equal(s1.ToCharSequence(), s2.ToCharSequence());
            }

            /// <summary>
            /// Compares two ICharSequence objects for binary equality.
            /// </summary>
            /// <param name="s1">s1 first sequence</param>
            /// <param name="s2">s2 second sequence</param>
            /// <returns>true if s1 contains the same text as s2.</returns>
            internal static bool Equal(ICharSequence s1, ICharSequence s2)
            {
                if (s1 == s2)
                {
                    return true;
                }
                int length = s1.Length;
                if (length != s2.Length)
                {
                    return false;
                }
                for (int i = 0; i < length; ++i)
                {
                    if (s1[i] != s2[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            /// <summary>
            /// Compares two ICharSequence subsequences for binary equality.
            /// </summary>
            /// <param name="s1">first sequence</param>
            /// <param name="start1">start offset in first sequence</param>
            /// <param name="limit1">limit offset in first sequence</param>
            /// <param name="s2">second sequence</param>
            /// <param name="start2">start offset in second sequence</param>
            /// <param name="limit2">limit offset in second sequence</param>
            /// <returns>true if s1.SubSequence(start1, limit1) contains the same text as s2.SubSequence(start2, limit2).</returns>
            public static bool Equal(string s1, int start1, int limit1,
                                    string s2, int start2, int limit2)
            {
                return Equal(s1.ToCharSequence(), start1, limit1,
                    s2.ToCharSequence(), start2, limit2);
            }

            /// <summary>
            /// Compares two ICharSequence subsequences for binary equality.
            /// </summary>
            /// <param name="s1">first sequence</param>
            /// <param name="start1">start offset in first sequence</param>
            /// <param name="limit1">limit offset in first sequence</param>
            /// <param name="s2">second sequence</param>
            /// <param name="start2">start offset in second sequence</param>
            /// <param name="limit2">limit offset in second sequence</param>
            /// <returns>true if s1.SubSequence(start1, limit1) contains the same text as s2.SubSequence(start2, limit2).</returns>
            public static bool Equal(string s1, int start1, int limit1,
                                    StringBuilder s2, int start2, int limit2)
            {
                return Equal(s1.ToCharSequence(), start1, limit1,
                    s2.ToCharSequence(), start2, limit2);
            }

            /// <summary>
            /// Compares two ICharSequence subsequences for binary equality.
            /// </summary>
            /// <param name="s1">first sequence</param>
            /// <param name="start1">start offset in first sequence</param>
            /// <param name="limit1">limit offset in first sequence</param>
            /// <param name="s2">second sequence</param>
            /// <param name="start2">start offset in second sequence</param>
            /// <param name="limit2">limit offset in second sequence</param>
            /// <returns>true if s1.SubSequence(start1, limit1) contains the same text as s2.SubSequence(start2, limit2).</returns>
            public static bool Equal(string s1, int start1, int limit1,
                                    char[] s2, int start2, int limit2)
            {
                return Equal(s1.ToCharSequence(), start1, limit1,
                    s2.ToCharSequence(), start2, limit2);
            }


            /// <summary>
            /// Compares two ICharSequence subsequences for binary equality.
            /// </summary>
            /// <param name="s1">first sequence</param>
            /// <param name="start1">start offset in first sequence</param>
            /// <param name="limit1">limit offset in first sequence</param>
            /// <param name="s2">second sequence</param>
            /// <param name="start2">start offset in second sequence</param>
            /// <param name="limit2">limit offset in second sequence</param>
            /// <returns>true if s1.SubSequence(start1, limit1) contains the same text as s2.SubSequence(start2, limit2).</returns>
            public static bool Equal(StringBuilder s1, int start1, int limit1,
                                    string s2, int start2, int limit2)
            {
                return Equal(s1.ToCharSequence(), start1, limit1,
                    s2.ToCharSequence(), start2, limit2);
            }

            /// <summary>
            /// Compares two ICharSequence subsequences for binary equality.
            /// </summary>
            /// <param name="s1">first sequence</param>
            /// <param name="start1">start offset in first sequence</param>
            /// <param name="limit1">limit offset in first sequence</param>
            /// <param name="s2">second sequence</param>
            /// <param name="start2">start offset in second sequence</param>
            /// <param name="limit2">limit offset in second sequence</param>
            /// <returns>true if s1.SubSequence(start1, limit1) contains the same text as s2.SubSequence(start2, limit2).</returns>
            public static bool Equal(StringBuilder s1, int start1, int limit1,
                                    StringBuilder s2, int start2, int limit2)
            {
                return Equal(s1.ToCharSequence(), start1, limit1,
                    s2.ToCharSequence(), start2, limit2);
            }

            /// <summary>
            /// Compares two ICharSequence subsequences for binary equality.
            /// </summary>
            /// <param name="s1">first sequence</param>
            /// <param name="start1">start offset in first sequence</param>
            /// <param name="limit1">limit offset in first sequence</param>
            /// <param name="s2">second sequence</param>
            /// <param name="start2">start offset in second sequence</param>
            /// <param name="limit2">limit offset in second sequence</param>
            /// <returns>true if s1.SubSequence(start1, limit1) contains the same text as s2.SubSequence(start2, limit2).</returns>
            public static bool Equal(StringBuilder s1, int start1, int limit1,
                                    char[] s2, int start2, int limit2)
            {
                return Equal(s1.ToCharSequence(), start1, limit1,
                    s2.ToCharSequence(), start2, limit2);
            }


            /// <summary>
            /// Compares two ICharSequence subsequences for binary equality.
            /// </summary>
            /// <param name="s1">first sequence</param>
            /// <param name="start1">start offset in first sequence</param>
            /// <param name="limit1">limit offset in first sequence</param>
            /// <param name="s2">second sequence</param>
            /// <param name="start2">start offset in second sequence</param>
            /// <param name="limit2">limit offset in second sequence</param>
            /// <returns>true if s1.SubSequence(start1, limit1) contains the same text as s2.SubSequence(start2, limit2).</returns>
            public static bool Equal(char[] s1, int start1, int limit1,
                                    string s2, int start2, int limit2)
            {
                return Equal(s1.ToCharSequence(), start1, limit1,
                    s2.ToCharSequence(), start2, limit2);
            }

            /// <summary>
            /// Compares two ICharSequence subsequences for binary equality.
            /// </summary>
            /// <param name="s1">first sequence</param>
            /// <param name="start1">start offset in first sequence</param>
            /// <param name="limit1">limit offset in first sequence</param>
            /// <param name="s2">second sequence</param>
            /// <param name="start2">start offset in second sequence</param>
            /// <param name="limit2">limit offset in second sequence</param>
            /// <returns>true if s1.SubSequence(start1, limit1) contains the same text as s2.SubSequence(start2, limit2).</returns>
            public static bool Equal(char[] s1, int start1, int limit1,
                                    StringBuilder s2, int start2, int limit2)
            {
                return Equal(s1.ToCharSequence(), start1, limit1,
                    s2.ToCharSequence(), start2, limit2);
            }

            /// <summary>
            /// Compares two ICharSequence subsequences for binary equality.
            /// </summary>
            /// <param name="s1">first sequence</param>
            /// <param name="start1">start offset in first sequence</param>
            /// <param name="limit1">limit offset in first sequence</param>
            /// <param name="s2">second sequence</param>
            /// <param name="start2">start offset in second sequence</param>
            /// <param name="limit2">limit offset in second sequence</param>
            /// <returns>true if s1.SubSequence(start1, limit1) contains the same text as s2.SubSequence(start2, limit2).</returns>
            public static bool Equal(char[] s1, int start1, int limit1,
                                    char[] s2, int start2, int limit2)
            {
                return Equal(s1.ToCharSequence(), start1, limit1,
                    s2.ToCharSequence(), start2, limit2);
            }

            /// <summary>
            /// Compares two ICharSequence subsequences for binary equality.
            /// </summary>
            /// <param name="s1">first sequence</param>
            /// <param name="start1">start offset in first sequence</param>
            /// <param name="limit1">limit offset in first sequence</param>
            /// <param name="s2">second sequence</param>
            /// <param name="start2">start offset in second sequence</param>
            /// <param name="limit2">limit offset in second sequence</param>
            /// <returns>true if s1.SubSequence(start1, limit1) contains the same text as s2.SubSequence(start2, limit2).</returns>
            internal static bool Equal(ICharSequence s1, int start1, int limit1,
                                    ICharSequence s2, int start2, int limit2)
            {
                if ((limit1 - start1) != (limit2 - start2))
                {
                    return false;
                }
                if (s1 == s2 && start1 == start2)
                {
                    return true;
                }
                while (start1 < limit1)
                {
                    if (s1[start1++] != s2[start2++])
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public Normalizer2Impl() { }

        private sealed class IsAcceptable : IAuthenticate
        {
            public bool IsDataVersionAcceptable(byte[] version)
            {
                return version[0] == 3;
            }
        }
        private static readonly IsAcceptable IS_ACCEPTABLE = new IsAcceptable();
        private static readonly int DATA_FORMAT = 0x4e726d32;  // "Nrm2"

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
                int trieLength = normTrie.GetSerializedLength();
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
                Trie2.Range range;
                while (trieIterator.MoveNext() && !(range = trieIterator.Current).LeadSurrogate)
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
                Trie2.Range range;
                while (trieIterator.MoveNext() && !(range = trieIterator.Current).LeadSurrogate)
                {
                    EnumNorm16PropertyStartsRange(range.StartCodePoint, range.EndCodePoint, range.Value, set);
                }
            }

            /* add Hangul LV syllables and LV+1 because of skippables */
            for (int c = Hangul.HANGUL_BASE; c < Hangul.HANGUL_LIMIT; c += Hangul.JAMO_T_COUNT)
            {
                set.Add(c);
                set.Add(c + 1);
            }
            set.Add(Hangul.HANGUL_LIMIT); /* add Hangul+1 to continue with other properties */
        }

        public void AddCanonIterPropertyStarts(UnicodeSet set)
        {
            /* add the start code point of each same-value range of the canonical iterator data trie */
            EnsureCanonIterData();
            // currently only used for the SEGMENT_STARTER property
            using (var trieIterator = canonIterData.GetEnumerator(segmentStarterMapper))
            {
                Trie2.Range range;
                while (trieIterator.MoveNext() && !(range = trieIterator.Current).LeadSurrogate)
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

        /**
         * Builds the canonical-iterator data for this instance.
         * This is required before any of {@link #isCanonSegmentStarter(int)} or
         * {@link #getCanonStartSet(int, UnicodeSet)} are called,
         * or else they crash.
         * @return this
         */
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
                        Trie2.Range range;
                        while (trieIterator.MoveNext() && !(range = trieIterator.Current).LeadSurrogate)
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

        /**
         * Returns the FCD data for code point c.
         * @param c A Unicode code point.
         * @return The lccc(c) in bits 15..8 and tccc(c) in bits 7..0.
         */
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
        /** Returns true if the single-or-lead code unit c might have non-zero FCD data. */
        public bool SingleLeadMightHaveNonZeroFCD16(int lead)
        {
            // 0<=lead<=0xffff
            byte bits = smallFCD[lead >> 8];
            if (bits == 0) { return false; }
            return ((bits >> ((lead >> 5) & 7)) & 1) != 0;
        }

        /** Gets the FCD value from the regular normalization data. */
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

        /**
         * Gets the decomposition for one code point.
         * @param c code point
         * @return c's decomposition, if it has one; returns null if it does not have a decomposition
         */
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
            return extraData.Substring(mapping, length); // mapping + length - mapping == length
        }

        /**
         * Gets the raw decomposition for one code point.
         * @param c code point
         * @return c's raw decomposition, if it has one; returns null if it does not have a decomposition
         */
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
                    return extraData.Substring(rawMapping - rm0, rm0); // rawMapping - rm0 - rawMapping == rm0
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
                return extraData.Substring(mapping, mLength); // mapping + mLength - mapping == mLength
            }
        }

        /**
         * Returns true if code point c starts a canonical-iterator string segment.
         * <b>{@link #ensureCanonIterData()} must have been called before this method,
         * or else this method will crash.</b>
         * @param c A Unicode code point.
         * @return true if c starts a canonical-iterator string segment.
         */
        public bool IsCanonSegmentStarter(int c)
        {
            return canonIterData.Get(c) >= 0;
        }
        /**
         * Returns true if there are characters whose decomposition starts with c.
         * If so, then the set is cleared and then filled with those characters.
         * <b>{@link #ensureCanonIterData()} must have been called before this method,
         * or else this method will crash.</b>
         * @param c A Unicode code point.
         * @param set A UnicodeSet to receive the characters whose decompositions
         *        start with c, if there are any.
         * @return true if there are characters whose decomposition starts with c.
         */
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
                    int syllable = Hangul.HANGUL_BASE + (c - Hangul.JAMO_L_BASE) * Hangul.JAMO_VT_COUNT;
                    set.Add(syllable, syllable + Hangul.JAMO_VT_COUNT - 1);
                }
                else
                {
                    AddComposites(GetCompositionsList(norm16), set);
                }
            }
            return true;
        }

        // Fixed norm16 values.
        public static readonly int MIN_YES_YES_WITH_CC = 0xfe02;
        public static readonly int JAMO_VT = 0xfe00;
        public static readonly int MIN_NORMAL_MAYBE_YES = 0xfc00;
        public static readonly int JAMO_L = 2;  // offset=1 hasCompBoundaryAfter=FALSE
        public static readonly int INERT = 1;  // offset=0 hasCompBoundaryAfter=TRUE

        // norm16 bit 0 is comp-boundary-after.
        public static readonly int HAS_COMP_BOUNDARY_AFTER = 1;
        public static readonly int OFFSET_SHIFT = 1;

        // For algorithmic one-way mappings, norm16 bits 2..1 indicate the
        // tccc (0, 1, >1) for quick FCC boundary-after tests.
        public static readonly int DELTA_TCCC_0 = 0;
        public static readonly int DELTA_TCCC_1 = 2;
        public static readonly int DELTA_TCCC_GT_1 = 4;
        public static readonly int DELTA_TCCC_MASK = 6;
        public static readonly int DELTA_SHIFT = 3;

        public static readonly int MAX_DELTA = 0x40;

        // Byte offsets from the start of the data, after the generic header.
        public static readonly int IX_NORM_TRIE_OFFSET = 0;
        public static readonly int IX_EXTRA_DATA_OFFSET = 1;
        public static readonly int IX_SMALL_FCD_OFFSET = 2;
        public static readonly int IX_RESERVED3_OFFSET = 3;
        public static readonly int IX_TOTAL_SIZE = 7;

        // Code point thresholds for quick check codes.
        public static readonly int IX_MIN_DECOMP_NO_CP = 8;
        public static readonly int IX_MIN_COMP_NO_MAYBE_CP = 9;

        // Norm16 value thresholds for quick check combinations and types of extra data.

        /** Mappings & compositions in [minYesNo..minYesNoMappingsOnly[. */
        public static readonly int IX_MIN_YES_NO = 10;
        /** Mappings are comp-normalized. */
        public static readonly int IX_MIN_NO_NO = 11;
        public static readonly int IX_LIMIT_NO_NO = 12;
        public static readonly int IX_MIN_MAYBE_YES = 13;

        /** Mappings only in [minYesNoMappingsOnly..minNoNo[. */
        public static readonly int IX_MIN_YES_NO_MAPPINGS_ONLY = 14;
        /** Mappings are not comp-normalized but have a comp boundary before. */
        public static readonly int IX_MIN_NO_NO_COMP_BOUNDARY_BEFORE = 15;
        /** Mappings do not have a comp boundary before. */
        public static readonly int IX_MIN_NO_NO_COMP_NO_MAYBE_CC = 16;
        /** Mappings to the empty string. */
        public static readonly int IX_MIN_NO_NO_EMPTY = 17;

        public static readonly int IX_MIN_LCCC_CP = 18;
        public static readonly int IX_COUNT = 20;

        public static readonly int MAPPING_HAS_CCC_LCCC_WORD = 0x80;
        public static readonly int MAPPING_HAS_RAW_MAPPING = 0x40;
        // unused bit 0x20;
        public static readonly int MAPPING_LENGTH_MASK = 0x1f;

        public static readonly int COMP_1_LAST_TUPLE = 0x8000;
        public static readonly int COMP_1_TRIPLE = 1;
        public static readonly int COMP_1_TRAIL_LIMIT = 0x3400;
        public static readonly int COMP_1_TRAIL_MASK = 0x7ffe;
        public static readonly int COMP_1_TRAIL_SHIFT = 9;  // 10-1 for the "triple" bit
        public static readonly int COMP_2_TRAIL_SHIFT = 6;
        public static readonly int COMP_2_TRAIL_MASK = 0xffc0;

        // higher-level functionality ------------------------------------------ ***

        // NFD without an NFD Normalizer2 instance.
        public StringBuilder Decompose(string s, StringBuilder dest)
        {
            return Decompose(s.ToCharSequence(), dest);
        }

        // NFD without an NFD Normalizer2 instance.
        public StringBuilder Decompose(StringBuilder s, StringBuilder dest)
        {
            return Decompose(s.ToCharSequence(), dest);
        }

        // NFD without an NFD Normalizer2 instance.
        public StringBuilder Decompose(char[] s, StringBuilder dest)
        {
            return Decompose(s.ToCharSequence(), dest);
        }

        // NFD without an NFD Normalizer2 instance.
        internal StringBuilder Decompose(ICharSequence s, StringBuilder dest)
        {
            Decompose(s, 0, s.Length, dest, s.Length);
            return dest;
        }

        /// <summary>
        /// Decomposes s[src, limit[ and writes the result to dest.
        /// limit can be NULL if src is NUL-terminated.
        /// destLengthEstimate is the initial dest buffer capacity and can be -1.
        /// </summary>
        public void Decompose(string s, int src, int limit, StringBuilder dest,
                   int destLengthEstimate)
        {
            Decompose(s.ToCharSequence(), src, limit, dest, destLengthEstimate);
        }

        /// <summary>
        /// Decomposes s[src, limit[ and writes the result to dest.
        /// limit can be NULL if src is NUL-terminated.
        /// destLengthEstimate is the initial dest buffer capacity and can be -1.
        /// </summary>
        public void Decompose(StringBuilder s, int src, int limit, StringBuilder dest,
                   int destLengthEstimate)
        {
            Decompose(s.ToCharSequence(), src, limit, dest, destLengthEstimate);
        }

        /// <summary>
        /// Decomposes s[src, limit[ and writes the result to dest.
        /// limit can be NULL if src is NUL-terminated.
        /// destLengthEstimate is the initial dest buffer capacity and can be -1.
        /// </summary>
        public void Decompose(char[] s, int src, int limit, StringBuilder dest,
                   int destLengthEstimate)
        {
            Decompose(s.ToCharSequence(), src, limit, dest, destLengthEstimate);
        }

        /// <summary>
        /// Decomposes s[src, limit[ and writes the result to dest.
        /// limit can be NULL if src is NUL-terminated.
        /// destLengthEstimate is the initial dest buffer capacity and can be -1.
        /// </summary>
        internal void Decompose(ICharSequence s, int src, int limit, StringBuilder dest,
                   int destLengthEstimate)
        {
            if (destLengthEstimate < 0)
            {
                destLengthEstimate = limit - src;
            }
            dest.Length = 0;
            ReorderingBuffer buffer = new ReorderingBuffer(this, dest, destLengthEstimate);
            Decompose(s, src, limit, buffer);
        }

        // Dual functionality:
        // buffer!=NULL: normalize
        // buffer==NULL: isNormalized/quickCheck/spanQuickCheckYes
        public int Decompose(string s, int src, int limit,
                             ReorderingBuffer buffer)
        {
            return Decompose(s.ToCharSequence(), src, limit, buffer);
        }

        // Dual functionality:
        // buffer!=NULL: normalize
        // buffer==NULL: isNormalized/quickCheck/spanQuickCheckYes
        public int Decompose(StringBuilder s, int src, int limit,
                             ReorderingBuffer buffer)
        {
            return Decompose(s.ToCharSequence(), src, limit, buffer);
        }

        // Dual functionality:
        // buffer!=NULL: normalize
        // buffer==NULL: isNormalized/quickCheck/spanQuickCheckYes
        public int Decompose(char[] s, int src, int limit,
                             ReorderingBuffer buffer)
        {
            return Decompose(s.ToCharSequence(), src, limit, buffer);
        }

        // Dual functionality:
        // buffer!=NULL: normalize
        // buffer==NULL: isNormalized/quickCheck/spanQuickCheckYes
        internal int Decompose(ICharSequence s, int src, int limit,
                         ReorderingBuffer buffer)
        {
            int minNoCP = minDecompNoCP;

            int prevSrc;
            int c = 0;
            int norm16 = 0;

            // only for quick check
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
                    if (buffer != null)
                    {
                        buffer.FlushAndAppendZeroCC(s, prevSrc, src);
                    }
                    else
                    {
                        prevCC = 0;
                        prevBoundary = src;
                    }
                }
                if (src == limit)
                {
                    break;
                }

                // Check one above-minimum, relevant code point.
                src += Character.CharCount(c);
                if (buffer != null)
                {
                    Decompose(c, norm16, buffer);
                }
                else
                {
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
            }
            return src;
        }

        public void DecomposeAndAppend(string s, bool doDecompose, ReorderingBuffer buffer)
        {
            DecomposeAndAppend(s.ToCharSequence(), doDecompose, buffer);
        }

        public void DecomposeAndAppend(StringBuilder s, bool doDecompose, ReorderingBuffer buffer)
        {
            DecomposeAndAppend(s.ToCharSequence(), doDecompose, buffer);
        }

        public void DecomposeAndAppend(char[] s, bool doDecompose, ReorderingBuffer buffer)
        {
            DecomposeAndAppend(s.ToCharSequence(), doDecompose, buffer);
        }

        internal void DecomposeAndAppend(ICharSequence s, bool doDecompose, ReorderingBuffer buffer)
        {
            int limit = s.Length;
            if (limit == 0)
            {
                return;
            }
            if (doDecompose)
            {
                Decompose(s, 0, limit, buffer);
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
            buffer.Append(s, 0, src, firstCC, prevCC);
            buffer.Append(s, src, limit);
        }

        // Very similar to composeQuickCheck(): Make the same changes in both places if relevant.
        // doCompose: normalize
        // !doCompose: isNormalized (buffer must be empty and initialized)
        public bool Compose(string s, int src, int limit,
                               bool onlyContiguous,
                               bool doCompose,
                               ReorderingBuffer buffer)
        {
            return Compose(s.ToCharSequence(), src, limit, onlyContiguous, doCompose, buffer);
        }

        // Very similar to composeQuickCheck(): Make the same changes in both places if relevant.
        // doCompose: normalize
        // !doCompose: isNormalized (buffer must be empty and initialized)
        public bool Compose(StringBuilder s, int src, int limit,
                               bool onlyContiguous,
                               bool doCompose,
                               ReorderingBuffer buffer)
        {
            return Compose(s.ToCharSequence(), src, limit, onlyContiguous, doCompose, buffer);
        }

        // Very similar to composeQuickCheck(): Make the same changes in both places if relevant.
        // doCompose: normalize
        // !doCompose: isNormalized (buffer must be empty and initialized)
        public bool Compose(char[] s, int src, int limit,
                               bool onlyContiguous,
                               bool doCompose,
                               ReorderingBuffer buffer)
        {
            return Compose(s.ToCharSequence(), src, limit, onlyContiguous, doCompose, buffer);
        }


        // Very similar to composeQuickCheck(): Make the same changes in both places if relevant.
        // doCompose: normalize
        // !doCompose: isNormalized (buffer must be empty and initialized)
        internal bool Compose(ICharSequence s, int src, int limit,
                           bool onlyContiguous,
                           bool doCompose,
                           ReorderingBuffer buffer)
        {
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
                                buffer.Append(s, prevBoundary, limit);
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
                                    buffer.Append(s, prevBoundary, prevSrc);
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
                                    buffer.Append(s, prevBoundary, prevSrc);
                                }
                                int mapping = norm16 >> OFFSET_SHIFT;
                                int length = extraData[mapping++] & MAPPING_LENGTH_MASK;
                                buffer.Append(extraData, mapping, mapping + length);
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
                                    buffer.Append(s, prevBoundary, prevSrc);
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
                        if (c < Hangul.JAMO_T_BASE)
                        {
                            // The current character is a Jamo Vowel,
                            // compose with previous Jamo L and following Jamo T.
                            char l = (char)(prev - Hangul.JAMO_L_BASE);
                            if (l < Hangul.JAMO_L_COUNT)
                            {
                                if (!doCompose)
                                {
                                    return false;
                                }
                                int t;
                                if (src != limit &&
                                        0 < (t = (s[src] - Hangul.JAMO_T_BASE)) &&
                                        t < Hangul.JAMO_T_COUNT)
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
                                    int syllable = Hangul.HANGUL_BASE +
                                        (l * Hangul.JAMO_V_COUNT + (c - Hangul.JAMO_V_BASE)) *
                                        Hangul.JAMO_T_COUNT + t;
                                    --prevSrc;  // Replace the Jamo L as well.
                                    if (prevBoundary != prevSrc)
                                    {
                                        buffer.Append(s, prevBoundary, prevSrc);
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
                            int syllable = prev + c - Hangul.JAMO_T_BASE;
                            --prevSrc;  // Replace the Hangul LV as well.
                            if (prevBoundary != prevSrc)
                            {
                                buffer.Append(s, prevBoundary, prevSrc);
                            }
                            buffer.Append((char)syllable);
                            prevBoundary = src;
                            continue;
                        }
                        // No matching context, or may need to decompose surrounding text first:
                        // Fall through to the slow path.
                    }
                    else if (norm16 > JAMO_VT)
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
                                        buffer.Append(s, prevBoundary, limit);
                                    }
                                    return true;
                                }
                                int prevCC = cc;
                                c = Character.CodePointAt(s, src);
                                n16 = normTrie.Get(c);
                                if (n16 >= MIN_YES_YES_WITH_CC)
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
                        buffer.Append(s, prevBoundary, prevSrc);
                    }
                    int recomposeStartIndex = buffer.Length;
                    // We know there is not a boundary here.
                    DecomposeShort(s, prevSrc, src, false /* !stopAtCompBoundary */, onlyContiguous,
                                   buffer);
                    // Decompose until the next boundary.
                    src = DecomposeShort(s, src, limit, true /* stopAtCompBoundary */, onlyContiguous,
                                         buffer);
                    Recompose(buffer, recomposeStartIndex, onlyContiguous);
                    if (!doCompose)
                    {
                        if (!buffer.Equals(s, prevSrc, src))
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
        public int ComposeQuickCheck(string s, int src, int limit,
                                 bool onlyContiguous, bool doSpan)
        {
            return ComposeQuickCheck(s.ToCharSequence(), src, limit, onlyContiguous, doSpan);
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
        public int ComposeQuickCheck(StringBuilder s, int src, int limit,
                                 bool onlyContiguous, bool doSpan)
        {
            return ComposeQuickCheck(s.ToCharSequence(), src, limit, onlyContiguous, doSpan);
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
        public int ComposeQuickCheck(char[] s, int src, int limit,
                                 bool onlyContiguous, bool doSpan)
        {
            return ComposeQuickCheck(s.ToCharSequence(), src, limit, onlyContiguous, doSpan);
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
        internal int ComposeQuickCheck(ICharSequence s, int src, int limit,
                                 bool onlyContiguous, bool doSpan)
        {
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

                int prevNorm16 = INERT;
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
                            if (norm16 < MIN_YES_YES_WITH_CC)
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

        public void ComposeAndAppend(string s,
                                 bool doCompose,
                                 bool onlyContiguous,
                                 ReorderingBuffer buffer)
        {
            ComposeAndAppend(s.ToCharSequence(), doCompose, onlyContiguous, buffer);
        }

        public void ComposeAndAppend(StringBuilder s,
                                 bool doCompose,
                                 bool onlyContiguous,
                                 ReorderingBuffer buffer)
        {
            ComposeAndAppend(s.ToCharSequence(), doCompose, onlyContiguous, buffer);
        }

        public void ComposeAndAppend(char[] s,
                                 bool doCompose,
                                 bool onlyContiguous,
                                 ReorderingBuffer buffer)
        {
            ComposeAndAppend(s.ToCharSequence(), doCompose, onlyContiguous, buffer);
        }

        internal void ComposeAndAppend(ICharSequence s,
                                 bool doCompose,
                                 bool onlyContiguous,
                                 ReorderingBuffer buffer)
        {
                int src = 0, limit = s.Length;
                if (!buffer.IsEmpty)
                {
                    int firstStarterInSrc = FindNextCompBoundary(s, 0, limit, onlyContiguous);
                    if (0 != firstStarterInSrc)
                    {
                        int lastStarterInDest = FindPreviousCompBoundary(buffer.StringBuilder.ToCharSequence(),
                                                                       buffer.Length, onlyContiguous);
                        StringBuilder middle = new StringBuilder((buffer.Length - lastStarterInDest) +
                                                               firstStarterInSrc + 16);
                        middle.Append(buffer.StringBuilder.ToString(), lastStarterInDest, buffer.Length - lastStarterInDest); // ICU4N : Fixed 2nd parameter
                        buffer.RemoveSuffix(buffer.Length - lastStarterInDest);
                        middle.Append(s, 0, firstStarterInSrc - 0);
                        Compose(middle, 0, middle.Length, onlyContiguous, true, buffer);
                        src = firstStarterInSrc;
                    }
                }
                if (doCompose)
                {
                    Compose(s, src, limit, onlyContiguous, true, buffer);
                }
                else
                {
                    buffer.Append(s, src, limit);
                }
        }

        // Dual functionality:
        // buffer!=NULL: normalize
        // buffer==NULL: isNormalized/quickCheck/spanQuickCheckYes
        public int MakeFCD(string s, int src, int limit, ReorderingBuffer buffer)
        {
            return MakeFCD(s.ToCharSequence(), src, limit, buffer);
        }

        // Dual functionality:
        // buffer!=NULL: normalize
        // buffer==NULL: isNormalized/quickCheck/spanQuickCheckYes
        public int MakeFCD(StringBuilder s, int src, int limit, ReorderingBuffer buffer)
        {
            return MakeFCD(s.ToCharSequence(), src, limit, buffer);
        }

        // Dual functionality:
        // buffer!=NULL: normalize
        // buffer==NULL: isNormalized/quickCheck/spanQuickCheckYes
        public int MakeFCD(char[] s, int src, int limit, ReorderingBuffer buffer)
        {
            return MakeFCD(s.ToCharSequence(), src, limit, buffer);
        }

        // Dual functionality:
        // buffer!=NULL: normalize
        // buffer==NULL: isNormalized/quickCheck/spanQuickCheckYes
        internal int MakeFCD(ICharSequence s, int src, int limit, ReorderingBuffer buffer)
        {
            // Note: In this function we use buffer->appendZeroCC() because we track
            // the lead and trail combining classes here, rather than leaving it to
            // the ReorderingBuffer.
            // The exception is the call to decomposeShort() which uses the buffer
            // in the normal way.

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
                        if (buffer != null)
                        {
                            buffer.FlushAndAppendZeroCC(s, prevSrc, src);
                        }
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
                    if (buffer != null)
                    {
                        // The last lccc==0 character is excluded from the
                        // flush-and-append call in case it needs to be modified.
                        buffer.FlushAndAppendZeroCC(s, prevSrc, prevBoundary);
                        buffer.Append(s, prevBoundary, src);
                    }
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
                    if (buffer != null)
                    {
                        buffer.AppendZeroCC(c);
                    }
                    prevFCD16 = fcd16;
                    continue;
                }
                else if (buffer == null)
                {
                    return prevBoundary;  // quick check "no"
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
                    DecomposeShort(s, prevBoundary, src, false, false, buffer);
                    prevBoundary = src;
                    prevFCD16 = 0;
                }
            }
            return src;
        }

        public void MakeFCDAndAppend(string s, bool doMakeFCD, ReorderingBuffer buffer)
        {
            MakeFCDAndAppend(s.ToCharSequence(), doMakeFCD, buffer);
        }

        public void MakeFCDAndAppend(StringBuilder s, bool doMakeFCD, ReorderingBuffer buffer)
        {
            MakeFCDAndAppend(s.ToCharSequence(), doMakeFCD, buffer);
        }

        public void MakeFCDAndAppend(char[] s, bool doMakeFCD, ReorderingBuffer buffer)
        {
            MakeFCDAndAppend(s.ToCharSequence(), doMakeFCD, buffer);
        }

        internal void MakeFCDAndAppend(ICharSequence s, bool doMakeFCD, ReorderingBuffer buffer)
        {
            int src = 0, limit = s.Length;
            if (!buffer.IsEmpty)
            {
                int firstBoundaryInSrc = FindNextFCDBoundary(s, 0, limit);
                if (0 != firstBoundaryInSrc)
                {
                    int lastBoundaryInDest = FindPreviousFCDBoundary(buffer.StringBuilder.ToCharSequence(),
                                                                   buffer.Length);
                    StringBuilder middle = new StringBuilder((buffer.Length - lastBoundaryInDest) +
                                                           firstBoundaryInSrc + 16);
                    middle.Append(buffer.StringBuilder.ToString(), lastBoundaryInDest, buffer.Length - lastBoundaryInDest); // ICU4N : Fixed 2nd parameter
                    buffer.RemoveSuffix(buffer.Length - lastBoundaryInDest);
                    middle.Append(s, 0, firstBoundaryInSrc - 0);
                    MakeFCD(middle, 0, middle.Length, buffer);
                    src = firstBoundaryInSrc;
                }
            }
            if (doMakeFCD)
            {
                MakeFCD(s, src, limit, buffer);
            }
            else
            {
                buffer.Append(s, src, limit);
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
        /**
         * A little faster and simpler than isDecompYesAndZeroCC() but does not include
         * the MaybeYes which combine-forward and have ccc=0.
         * (Standard Unicode 10 normalization does not have such characters.)
         */
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

        /**
         * @return index into maybeYesCompositions, or -1
         */
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
        /**
         * @return index into maybeYesCompositions
         */
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
        /**
         * @param c code point must have compositions
         * @return index into maybeYesCompositions
         */
        private int GetCompositionsList(int norm16)
        {
            return IsDecompYes(norm16) ?
                    GetCompositionsListForDecompYes(norm16) :
                    GetCompositionsListForComposite(norm16);
        }

        // Decompose a short piece of text which is likely to contain characters that
        // fail the quick check loop and/or where the quick check loop's overhead
        // is unlikely to be amortized.
        // Called by the compose() and makeFCD() implementations.
        // Public in Java for collation implementation code.
        private int DecomposeShort(
                ICharSequence s, int src, int limit,
                bool stopAtCompBoundary, bool onlyContiguous,
                ReorderingBuffer buffer)
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
                Decompose(c, norm16, buffer);
                if (stopAtCompBoundary && Norm16HasCompBoundaryAfter(norm16, onlyContiguous))
                {
                    return src;
                }
            }
            return src;
        }
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

        /**
         * Finds the recomposition result for
         * a forward-combining "lead" character,
         * specified with a pointer to its compositions list,
         * and a backward-combining "trail" character.
         *
         * <p>If the lead and trail characters combine, then this function returns
         * the following "compositeAndFwd" value:
         * <pre>
         * Bits 21..1  composite character
         * Bit      0  set if the composite is a forward-combining starter
         * </pre>
         * otherwise it returns -1.
         *
         * <p>The compositions list has (trail, compositeAndFwd) pair entries,
         * encoded as either pairs or triples of 16-bit units.
         * The last entry has the high bit of its first unit set.
         *
         * <p>The list is sorted by ascending trail characters (there are no duplicates).
         * A linear search is used.
         *
         * <p>See normalizer2impl.h for a more detailed description
         * of the compositions list format.
         */
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
        /**
         * @param list some character's compositions list
         * @param set recursively receives the composites from these compositions
         */
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
        /*
         * Recomposes the buffer text starting at recomposeStartIndex
         * (which is in NFD - decomposed and canonically ordered),
         * and truncates the buffer contents.
         *
         * Note that recomposition never lengthens the text:
         * Any character consists of either one or two code units;
         * a composition may contain at most one more code unit than the original starter,
         * while the combining mark that is removed has at least one code unit.
         */
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
                        if (c < Hangul.JAMO_T_BASE)
                        {
                            // c is a Jamo Vowel, compose with previous Jamo L and following Jamo T.
                            char prev = (char)(sb[starter] - Hangul.JAMO_L_BASE);
                            if (prev < Hangul.JAMO_L_COUNT)
                            {
                                pRemove = p - 1;
                                char syllable = (char)
                                    (Hangul.HANGUL_BASE +
                                     (prev * Hangul.JAMO_V_COUNT + (c - Hangul.JAMO_V_BASE)) *
                                     Hangul.JAMO_T_COUNT);
                                char t;
                                if (p != sb.Length && (t = (char)(sb[p] - Hangul.JAMO_T_BASE)) < Hangul.JAMO_T_COUNT)
                                {
                                    ++p;
                                    syllable += t;  // The next character was a Jamo T.
                                }
                                //sb.setCharAt(starter, syllable);
                                sb[starter] = syllable;
                                // remove the Jamo V/T
                                sb.Delete(pRemove, p);
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
                        sb.Delete(pRemove, p);
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
                    b -= Hangul.JAMO_V_BASE;
                    if (0 <= b && b < Hangul.JAMO_V_COUNT)
                    {
                        return
                            (Hangul.HANGUL_BASE +
                             ((a - Hangul.JAMO_L_BASE) * Hangul.JAMO_V_COUNT + b) *
                             Hangul.JAMO_T_COUNT);
                    }
                    else
                    {
                        return -1;
                    }
                }
                else if (IsHangulLV(norm16))
                {
                    b -= Hangul.JAMO_T_BASE;
                    if (0 < b && b < Hangul.JAMO_T_COUNT)
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

        /**
         * Does c have a composition boundary before it?
         * True if its decomposition begins with a character that has
         * ccc=0 && NFC_QC=Yes (isCompYesAndZeroCC()).
         * As a shortcut, this is true if c itself has ccc=0 && NFC_QC=Yes
         * (isCompYesAndZeroCC()) so we need not decompose.
         */
        private bool HasCompBoundaryBefore(int c, int norm16)
        {
            return c < minCompNoMaybeCP || Norm16HasCompBoundaryBefore(norm16);
        }
        private bool Norm16HasCompBoundaryBefore(int norm16)
        {
            return norm16 < minNoNoCompNoMaybeCC || IsAlgorithmicNoNo(norm16);
        }
        private bool HasCompBoundaryBefore(ICharSequence s, int src, int limit)
        {
            return src == limit || HasCompBoundaryBefore(Character.CodePointAt(s, src));
        }
        private bool Norm16HasCompBoundaryAfter(int norm16, bool onlyContiguous)
        {
            return (norm16 & HAS_COMP_BOUNDARY_AFTER) != 0 &&
                (!onlyContiguous || IsTrailCC01ForCompBoundaryAfter(norm16));
        }
        private bool HasCompBoundaryAfter(ICharSequence s, int start, int p, bool onlyContiguous)
        {
            return start == p || HasCompBoundaryAfter(Character.CodePointBefore(s, p), onlyContiguous);
        }
        /** For FCC: Given norm16 HAS_COMP_BOUNDARY_AFTER, does it have tccc<=1? */
        private bool IsTrailCC01ForCompBoundaryAfter(int norm16)
        {
            return IsInert(norm16) || (IsDecompNoAlgorithmic(norm16) ?
                (norm16 & DELTA_TCCC_MASK) <= DELTA_TCCC_1 : extraData[norm16 >> OFFSET_SHIFT] <= 0x1ff);
        }

        private int FindPreviousCompBoundary(ICharSequence s, int p, bool onlyContiguous)
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
        private int FindNextCompBoundary(ICharSequence s, int p, int limit, bool onlyContiguous)
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

        private int FindPreviousFCDBoundary(ICharSequence s, int p)
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
        private int FindNextFCDBoundary(ICharSequence s, int p, int limit)
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

        private int GetPreviousTrailCC(ICharSequence s, int start, int p)
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
        private static readonly int CANON_NOT_SEGMENT_STARTER = unchecked((int)0x80000000);
        private static readonly int CANON_HAS_COMPOSITIONS = 0x40000000;
        private static readonly int CANON_HAS_SET = 0x200000;
        private static readonly int CANON_VALUE_MASK = 0x1fffff;
    }
}
