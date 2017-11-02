using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Text
{
    public interface IReplaceable
    {
        /**
         * Returns the number of 16-bit code units in the text.
         * @return number of 16-bit code units in text
         * @stable ICU 2.0
         */
        int Length { get; }

        /**
         * Returns the 16-bit code unit at the given offset into the text.
         * @param offset an integer between 0 and <code>length()</code>-1
         * inclusive
         * @return 16-bit code unit of text at given offset
         * @stable ICU 2.0
         */
        //char CharAt(int offset); // ICU4N TODO: API Make this into this[index] ?
        char this[int index] { get; }

        /**
         * Returns the 32-bit code point at the given 16-bit offset into
         * the text.  This assumes the text is stored as 16-bit code units
         * with surrogate pairs intermixed.  If the offset of a leading or
         * trailing code unit of a surrogate pair is given, return the
         * code point of the surrogate pair.
         *
         * <p>Most subclasses can return
         * <code>com.ibm.icu.text.UTF16.charAt(this, offset)</code>.
         * @param offset an integer between 0 and <code>length()</code>-1
         * inclusive
         * @return 32-bit code point of text at given offset
         * @stable ICU 2.0
         */
        int Char32At(int offset);

        /**
         * Copies characters from this object into the destination
         * character array.  The first character to be copied is at index
         * <code>srcStart</code>; the last character to be copied is at
         * index <code>srcLimit-1</code> (thus the total number of
         * characters to be copied is <code>srcLimit-srcStart</code>). The
         * characters are copied into the subarray of <code>dst</code>
         * starting at index <code>dstStart</code> and ending at index
         * <code>dstStart + (srcLimit-srcStart) - 1</code>.
         *
         * @param srcStart the beginning index to copy, inclusive; <code>0
         * &lt;= start &lt;= limit</code>.
         * @param srcLimit the ending index to copy, exclusive;
         * <code>start &lt;= limit &lt;= length()</code>.
         * @param dst the destination array.
         * @param dstStart the start offset in the destination array.
         * @stable ICU 2.0
         */
        //void GetChars(int srcStart, int srcLimit, char dst[], int dstStart);
        void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count);

        /**
         * Replaces a substring of this object with the given text.
         *
         * <p>Subclasses must ensure that if the text between start and
         * limit is equal to the replacement text, that replace has no
         * effect. That is, any metadata
         * should be unaffected. In addition, subclasses are encouraged to
         * check for initial and trailing identical characters, and make a
         * smaller replacement if possible. This will preserve as much
         * metadata as possible.
         * @param start the beginning index, inclusive; <code>0 &lt;= start
         * &lt;= limit</code>.
         * @param limit the ending index, exclusive; <code>start &lt;= limit
         * &lt;= length()</code>.
         * @param text the text to replace characters <code>start</code>
         * to <code>limit - 1</code>
         * @stable ICU 2.0
         */
        void Replace(int start, int limit, string text);

        /**
         * Replaces a substring of this object with the given text.
         *
         * <p>Subclasses must ensure that if the text between start and
         * limit is equal to the replacement text, that replace has no
         * effect. That is, any metadata
         * should be unaffected. In addition, subclasses are encouraged to
         * check for initial and trailing identical characters, and make a
         * smaller replacement if possible. This will preserve as much
         * metadata as possible.
         * @param start the beginning index, inclusive; <code>0 &lt;= start
         * &lt;= limit</code>.
         * @param limit the ending index, exclusive; <code>start &lt;= limit
         * &lt;= length()</code>.
         * @param chars the text to replace characters <code>start</code>
         * to <code>limit - 1</code>
         * @param charsStart the beginning index into <code>chars</code>,
         * inclusive; <code>0 &lt;= start &lt;= limit</code>.
         * @param charsLen the number of characters of <code>chars</code>.
         * @stable ICU 2.0
         */
        void Replace(int start, int limit, char[] chars,
                     int charsStart, int charsLen);
        // Note: We use length rather than limit to conform to StringBuffer
        // and System.arraycopy.

        /**
         * Copies a substring of this object, retaining metadata.
         * This method is used to duplicate or reorder substrings.
         * The destination index must not overlap the source range.
         * If <code>hasMetaData()</code> returns false, subclasses
         * may use the naive implementation:
         *
         * <pre> char[] text = new char[limit - start];
         * getChars(start, limit, text, 0);
         * replace(dest, dest, text, 0, limit - start);</pre>
         *
         * @param start the beginning index, inclusive; <code>0 &lt;= start &lt;=
         * limit</code>.
         * @param limit the ending index, exclusive; <code>start &lt;= limit &lt;=
         * length()</code>.
         * @param dest the destination index.  The characters from
         * <code>start..limit-1</code> will be copied to <code>dest</code>.
         * Implementations of this method may assume that <code>dest &lt;= start ||
         * dest &gt;= limit</code>.
         * @stable ICU 2.0
         */
        void Copy(int start, int limit, int dest);

        /**R
         * Returns true if this object contains metadata.  If a
         * Replaceable object has metadata, calls to the Replaceable API
         * must be made so as to preserve metadata.  If it does not, calls
         * to the Replaceable API may be optimized to improve performance.
         * @return true if this object contains metadata
         * @stable ICU 2.2
         */
        bool HasMetaData { get; }
    }
}
