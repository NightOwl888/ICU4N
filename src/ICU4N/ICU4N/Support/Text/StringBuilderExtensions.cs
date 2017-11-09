using System;
using System.Text;

namespace ICU4N.Support.Text
{
    public static class StringBuilderExtensions
    {
        /// <summary>
        /// Causes this character sequence to be replaced by the reverse of
        /// the sequence. If there are any surrogate pairs included in the
        /// sequence, these are treated as single characters for the
        /// reverse operation. Thus, the order of the high-low surrogates
        /// is never reversed.
        /// <para/>
        /// Let <c>n</c> be the character length of this character sequence
        /// (not the length in <see cref="char"/> values) just prior to
        /// execution of the <see cref="Reverse"/> method. Then the
        /// character at index <c>k</c> in the new character sequence is
        /// equal to the character at index <c>n-k-1</c> in the old
        /// character sequence.
        /// <para/>
        /// Note that the reverse operation may result in producing
        /// surrogate pairs that were unpaired low-surrogates and
        /// high-surrogates before the operation. For example, reversing
        /// "&#92;uDC00&#92;uD800" produces "&#92;uD800&#92;uDC00" which is
        /// a valid surrogate pair.
        /// </summary>
        /// <param name="text">this <see cref="StringBuilder"/></param>
        /// <returns>a reference to this <see cref="StringBuilder"/>.</returns>
        public static StringBuilder Reverse(this StringBuilder text)
        {
            bool hasSurrogate = false;
            int codePointCount = text.Length;
            int n = text.Length - 1;
            for (int j = (n - 1) >> 1; j >= 0; --j)
            {
                char temp = text[j];
                char temp2 = text[n - j];
                if (!hasSurrogate)
                {
                    hasSurrogate = (temp >= Character.MIN_SURROGATE && temp <= Character.MAX_SURROGATE)
                        || (temp2 >= Character.MIN_SURROGATE && temp2 <= Character.MAX_SURROGATE);
                }
                text[j] = temp2;
                text[n - j] = temp;
            }
            if (hasSurrogate)
            {
                // Reverse back all valid surrogate pairs
                for (int i = 0; i < text.Length - 1; i++)
                {
                    char c2 = text[i];
                    if (char.IsLowSurrogate(c2))
                    {
                        char c1 = text[i + 1];
                        if (char.IsHighSurrogate(c1))
                        {
                            text[i++] = c1;
                            text[i] = c2;
                        }
                    }
                }
            }

            return text;
        }

        /// <summary>
        /// Returns the number of Unicode code points in the specified text
        /// range of this <see cref="StringBuilder"/>. The text range begins at the specified
        /// <paramref name="beginIndex"/> and extends to the <see cref="char"/> at
        /// index <c>endIndex - 1</c>. Thus the length (in
        /// <see cref="char"/>s) of the text range is
        /// <c>endIndex-beginIndex</c>. Unpaired surrogates within
        /// this sequence count as one code point each.
        /// </summary>
        /// <param name="text">this <see cref="StringBuilder"/></param>
        /// <param name="beginIndex">the index to the first <see cref="char"/> of the text range.</param>
        /// <param name="endIndex">the index after the last <see cref="char"/> of the text range.</param>
        /// <returns>the number of Unicode code points in the specified text range.</returns>
        /// <exception cref="IndexOutOfRangeException">
        /// if the <paramref name="beginIndex"/> is negative, or <paramref name="endIndex"/>
        /// is larger than the length of this sequence, or
        /// <paramref name="beginIndex"/> is larger than <paramref name="endIndex"/>.
        /// </exception>
        public static int CodePointCount(this StringBuilder text, int beginIndex, int endIndex)
        {
            if (beginIndex < 0 || endIndex > text.Length || beginIndex > endIndex)
            {
                throw new IndexOutOfRangeException();
            }
            return Character.CodePointCountImpl(text.GetChars(), beginIndex, endIndex - beginIndex);
        }

        /// <summary>
        /// Returns the character (Unicode code point) at the specified index. 
        /// The index refers to char values (Unicode code units) and ranges from 0 to Length - 1.
        /// <para/>
        /// If the char value specified at the given index is in the high-surrogate range, 
        /// the following index is less than the length of this sequence, and the char value 
        /// at the following index is in the low-surrogate range, then the 
        /// supplementary code point corresponding to this surrogate pair is returned. 
        /// Otherwise, the char value at the given index is returned.
        /// </summary>
        /// <param name="text">this <see cref="StringBuilder"/></param>
        /// <param name="index">the index to the char values</param>
        /// <returns>the code point value of the character at the index</returns>
        /// <exception cref="IndexOutOfRangeException">if the index argument is negative or not less than the length of this sequence.</exception>
        public static int CodePointAt(this StringBuilder text, int index)
        {
            if ((index < 0) || (index >= text.Length))
            {
                throw new IndexOutOfRangeException();
            }
            return Character.CodePointAt(text.ToString(), index);
        }

        /// <summary>
        /// Copies the array from the <see cref="StringBuilder"/> into a new array
        /// and returns it.
        /// </summary>
        /// <param name="text">this <see cref="StringBuilder"/></param>
        /// <returns></returns>
        public static char[] GetChars(this StringBuilder text)
        {
            char[] chars = new char[text.Length];
            text.CopyTo(0, chars, 0, text.Length);
            return chars;
        }

        /// <summary>
        /// Appends the string representation of the <paramref name="codePoint"/>
        /// argument to this sequence.
        /// 
        /// <para>
        /// The argument is appended to the contents of this sequence.
        /// The length of this sequence increases by <see cref="Character.CharCount(int)"/>.
        /// </para>
        /// <para>
        /// The overall effect is exactly as if the argument were
        /// converted to a <see cref="char"/> array by the method
        /// <see cref="Character.ToChars(int)"/> and the character in that array
        /// were then <see cref="StringBuilder.Append(char[])">appended</see> to this 
        /// <see cref="StringBuilder"/>.
        /// </para>
        /// </summary>
        /// <param name="text">This <see cref="StringBuilder"/>.</param>
        /// <param name="codePoint">a Unicode code point</param>
        /// <returns>a reference to this object.</returns>
        public static StringBuilder AppendCodePoint(this StringBuilder text, int codePoint)
        {
            text.Append(Character.ToChars(codePoint));
            return text;
        }

        /// <summary>
        /// Searches for the first index of the specified character. The search for
        /// the character starts at the beginning and moves towards the end.
        /// </summary>
        /// <param name="text">This <see cref="StringBuilder"/>.</param>
        /// <param name="value">The string to find.</param>
        /// <returns>The index of the specified character, or -1 if the character isn't found.</returns>
        public static int IndexOf(this StringBuilder text, string value)
        {
            return IndexOf(text, value, 0);
        }

        /// <summary>
        /// Searches for the index of the specified character. The search for the
        /// character starts at the specified offset and moves towards the end.
        /// </summary>
        /// <param name="text">This <see cref="StringBuilder"/>.</param>
        /// <param name="value">The string to find.</param>
        /// <param name="startIndex">The starting offset.</param>
        /// <returns>The index of the specified character, or -1 if the character isn't found.</returns>
        public static int IndexOf(this StringBuilder text, string value, int startIndex)
        {
            if (text == null)
                throw new ArgumentNullException("text");
            if (value == null)
                throw new ArgumentNullException("value");

            int index;
            int length = value.Length;
            int maxSearchLength = (text.Length - length) + 1;

            for (int i = startIndex; i < maxSearchLength; ++i)
            {
                if (text[i] == value[0])
                {
                    index = 1;
                    while ((index < length) && (text[i + index] == value[index]))
                        ++index;

                    if (index == length)
                        return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Searches for the last index of the specified character. The search for
        /// the character starts at the end and moves towards the beginning.
        /// </summary>
        /// <param name="text">This <see cref="StringBuilder"/>.</param>
        /// <param name="value">The string to find.</param>
        /// <returns>The index of the specified character, -1 if the character isn't found.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="value"/> is <c>null</c>.</exception>
        public static int LastIndexOf(this StringBuilder text, string value)
        {
            if (text == null)
                throw new ArgumentNullException("text");
            if (value == null)
                throw new ArgumentNullException("value");

            return LastIndexOf(text, value, text.Length);
        }

        /// <summary>
        /// Searches for the index of the specified character. The search for the
        /// character starts at the specified offset and moves towards the beginning.
        /// </summary>
        /// <param name="text">This <see cref="StringBuilder"/>.</param>
        /// <param name="value">The string to find.</param>
        /// <param name="start">The starting offset.</param>
        /// <returns>The index of the specified character, -1 if the character isn't found.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="value"/> is <c>null</c>.</exception>
        public static int LastIndexOf(this StringBuilder text, string value, int start)
        {
            if (text == null)
                throw new ArgumentNullException("text");
            if (value == null)
                throw new ArgumentNullException("value");

            int subCount = value.Length;
            if (subCount <= text.Length && start >= 0)
            {
                if (subCount > 0)
                {
                    if (start > text.Length - subCount)
                    {
                        start = text.Length - subCount; // count and subCount are both
                    }
                    // >= 1
                    // TODO optimize charAt to direct array access
                    char firstChar = value[0];
                    while (true)
                    {
                        int i = start;
                        bool found = false;
                        for (; i >= 0; --i)
                        {
                            if (text[i] == firstChar)
                            {
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                        {
                            return -1;
                        }
                        int o1 = i, o2 = 0;
                        while (++o2 < subCount
                                && text[++o1] == value[o2])
                        {
                            // Intentionally empty
                        }
                        if (o2 == subCount)
                        {
                            return i;
                        }
                        start = i - 1;
                    }
                }
                return start < text.Length ? start : text.Length;
            }
            return -1;
        }

        /// <summary>
        /// Convenience method to wrap a string in a <see cref="StringBuilderCharSequence"/>
        /// so a <see cref="StringBuilder"/> can be used as <see cref="ICharSequence"/> in .NET.
        /// </summary>
        internal static ICharSequence ToCharSequence(this StringBuilder text)
        {
            return new StringBuilderCharSequence(text);
        }

        /// <summary>
        /// Appends the give <see cref="ICharSequence"/> to this <see cref="StringBuilder"/>.
        /// </summary>
        internal static StringBuilder Append(this StringBuilder text, ICharSequence csq)
        {
            if (csq is StringCharSequence)
                text.Append(((StringCharSequence)csq).Value);
            else if (csq is StringBuilderCharSequence)
                text.Append(((StringBuilderCharSequence)csq).Value);
            else if (csq is CharArrayCharSequence)
                text.Append(((CharArrayCharSequence)csq).Value);
            else
                text.Append(csq.ToString());
            return text;
        }

        /// <summary>
        /// Appends the given <see cref="ICharSequence"/> to this <see cref="StringBuilder"/>.
        /// </summary>
        // .NET Port note: This uses the .NET style length as the 3rd parameter. All callers need to account for this by subtracting (end - start).
        internal static StringBuilder Append(this StringBuilder text, ICharSequence csq, int start, int count)
        {
            if (csq == null)
            {
                text.Append("null");
                return text;
            }

            if ((start < 0) || (start + count > csq.Length))
                throw new IndexOutOfRangeException(
                    "start " + start + ", length " + count + ", csq.Length "
                    + csq.Length);
            int end = start + count;
            for (int i = start; i < end; i++)
                text.Append(csq[i]);
            return text;
        }

        ///// <summary>
        ///// Appends the given <see cref="ICharSequence"/> to this <see cref="StringBuilder"/>.
        ///// </summary>
        //internal static StringBuilder Append(this StringBuilder text, ICharSequence csq, int start, int end) // ICU4N TODO: Weird to have end vs length, but using length means we need a conversion everywhere. Probably be best to use length anyway for consistency with .NET.
        //{
        //    if (csq == null)
        //        csq = "null".ToCharSequence();
        //    if ((start < 0) || (start > end) || (end > csq.Length))
        //        throw new IndexOutOfRangeException(
        //            "start " + start + ", end " + end + ", s.length() "
        //            + csq.Length);
        //    int len = end - start;
        //    for (int i = start, j = text.Length; i < end; i++, j++)
        //        text.Append(csq[i]);
        //    return text;
        //}

        /// <summary>
        /// Appends the given <see cref="StringBuilder"/> to this <see cref="StringBuilder"/>.
        /// </summary>
        internal static StringBuilder Append(this StringBuilder text, StringBuilder csq)
        {
            if (csq == null)
                text.Append("null");
            else
                text.Append(csq.ToString());
            return text;
        }

        ///// <summary>
        ///// Appends the given <see cref="StringBuilder"/> to this <see cref="StringBuilder"/>.
        ///// </summary>
        //internal static StringBuilder Append(this StringBuilder text, StringBuilder csq, int start, int end) 
        //{
        //    if (csq == null)
        //        csq = new StringBuilder("null");
        //    if ((start < 0) || (start > end) || (end > csq.Length))
        //        throw new IndexOutOfRangeException(
        //            "start " + start + ", end " + end + ", s.Length "
        //            + csq.Length);
        //    int len = end - start;
        //    for (int i = start, j = text.Length; i < end; i++, j++)
        //        text.Append(csq[i]);
        //    return text;
        //}

        /// <summary>
        /// Appends the given <see cref="StringBuilder"/> to this <see cref="StringBuilder"/>.
        /// </summary>
        internal static StringBuilder Append(this StringBuilder text, StringBuilder csq, int start, int count)
        {
            if (csq == null)
            {
                text.Append("null");
                return text;
            }

            if ((start < 0) || (start + count > csq.Length))
                throw new IndexOutOfRangeException(
                    "start " + start + ", length " + count + ", csq.Length "
                    + csq.Length);
            int end = start + count;
            for (int i = start; i < end; i++)
                text.Append(csq[i]);
            return text;
        }

        internal static StringBuilder Delete(this StringBuilder text, int start, int end)
        {
            if (start < 0)
                throw new IndexOutOfRangeException(nameof(start));

            int length = end - start;
            if (start + length > text.Length)
            {
                length = text.Length - start;
            }
            if (length > 0)
            {
                text.Remove(start, length);
            }

            return text;
        }

        internal static ICharSequence SubSequence(this StringBuilder text, int start, int end)
        {
            // From Apache Harmony String class
            if (start == 0 && end == text.Length)
            {
                return text.ToCharSequence();
            }
            if (start < 0)
            {
                throw new IndexOutOfRangeException(nameof(start));
            }
            else if (start > end)
            {
                throw new IndexOutOfRangeException("end - start");
            }
            else if (end > text.Length)
            {
                throw new IndexOutOfRangeException(nameof(end));
            }

            return text.ToString(start, end - start).ToCharSequence();
        }

        /// <summary>
        /// Replaces the specified subsequence in this builder with the specified
        /// string.
        /// </summary>
        /// <param name="text">this builder.</param>
        /// <param name="start">the inclusive begin index.</param>
        /// <param name="end">the exclusive end index.</param>
        /// <param name="str">the replacement string.</param>
        /// <returns>this builder.</returns>
        /// <exception cref="IndexOutOfRangeException">
        /// if <paramref name="start"/> is negative, greater than the current
        /// <see cref="StringBuilder.Length"/> or greater than <paramref name="end"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">if <paramref name="str"/> is <c>null</c>.</exception>
        public static StringBuilder Replace(this StringBuilder text, int start, int end, string str)
        {
            if (start >= 0)
            {
                if (end > text.Length)
                {
                    end = text.Length;
                }
                if (end > start)
                {
                    int stringLength = str.Length;
                    int diff = end - start - stringLength;
                    char[] value = new char[text.Length + str.Length];
                    text.CopyTo(0, value, 0, text.Length);
                    if (diff > 0)
                    { // replacing with fewer characters
                        //if (!shared)
                        //{
                            // index == count case is no-op
                            System.Array.Copy(value, end, value, start
                                    + stringLength, text.Length - end);
                        //}
                        //else
                        //{
                        //    char[] newData = new char[value.length];
                        //    System.arraycopy(value, 0, newData, 0, start);
                        //    // index == count case is no-op
                        //    System.arraycopy(value, end, newData, start
                        //            + stringLength, count - end);
                        //    value = newData;
                        //    shared = false;
                        //}
                    }
                    //else if (diff < 0)
                    //{
                    //    // replacing with more characters...need some room
                    //    text.Move(-diff, end);
                    //}
                    //else if (shared)
                    //{
                    //    value = value.clone();
                    //    shared = false;
                    //}
                    //str.GetChars(0, stringLength, value, start);
                    str.CopyTo(0, value, start, stringLength);
                    text.Length -= diff;
                    // copy the chars based on the new length
                    for (int i = 0; i < text.Length; i++)
                    {
                        text[i] = value[i];
                    }
                    return text;
                }
                if (start == end)
                {
                    if (str == null)
                    {
                        throw new ArgumentNullException(nameof(str));
                    }
                    text.Insert(start, str);
                    return text;
                }
            }
            throw new IndexOutOfRangeException();
        }

        /// <summary>
        /// Returns the index that is offset <paramref name="codePointOffset"/> code points from
        /// <paramref name="index"/>.
        /// </summary>
        /// <param name="text">This <see cref="StringBuilder"/>.</param>
        /// <param name="index">The index to calculate the offset from.</param>
        /// <param name="codePointOffset">The number of code points to count.</param>
        /// <returns>The index that is <paramref name="codePointOffset"/> code points away from <paramref name="index"/>.</returns>
        /// <seealso cref="Character"/>
        /// <seealso cref="Character.OffsetByCodePoints(char[], int, int, int, int)"/>
        public static int OffsetByCodePoints(this StringBuilder text, int index, int codePointOffset)
        {
            var chars = text.GetChars();
            return Character.OffsetByCodePoints(chars, 0, chars.Length, index,
                codePointOffset);
        }

        /// <summary>
        /// Retrieves the Unicode code point value that precedes the <paramref name="index"/>.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="index">The index to the <see cref="char"/> code unit within this object.</param>
        /// <returns>The Unicode code point value.</returns>
        /// <seealso cref="Character"/>
        /// <seealso cref="Character.CodePointBefore(char[], int)"/>
        public static int CodePointBefore(this StringBuilder text, int index)
        {
            if (index < 1 || index > text.Length)
            {
                throw new IndexOutOfRangeException(nameof(index));
            }
            return Character.CodePointBefore(text, index);
        }

        /// <summary>
        /// Convenience method to wrap a <see cref="StringBuilder"/> in an
        /// <see cref="StringBuilderAppendable"/> adapter class so it can be
        /// used with the <see cref="IAppendable"/> interface.
        /// </summary>
        /// <param name="text">This <see cref="StringBuilder"/>.</param>
        /// <returns>An <see cref="StringBuilderAppendable"/>.</returns>
        internal static IAppendable ToAppendable(this StringBuilder text)
        {
            return new StringBuilderAppendable(text);
        }
    }
}
