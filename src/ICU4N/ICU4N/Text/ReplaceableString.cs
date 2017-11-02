using ICU4N.Support.Text;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Text
{
    /// <summary>
    /// <see cref="ReplaceableString"/> is an adapter class that implements the
    /// <see cref="IReplaceable"/> API around an ordinary <see cref="StringBuffer"/>.
    /// </summary>
    /// <remarks>
    /// <em>Note:</em> This class does not support attributes and is not
    /// intended for general use.  Most clients will need to implement
    /// <see cref="IReplaceable"/> in their text representation class.
    /// </remarks>
    /// <see cref="IReplaceable"/>
    /// <author>Alan Liu</author>
    /// <stable>ICU 2.0</stable>
    public class ReplaceableString : IReplaceable
    {
        private StringBuffer buf;

        /**
         * Construct a new object with the given initial contents.
         * @param str initial contents
         * @stable ICU 2.0
         */
        public ReplaceableString(string str)
        {
            buf = new StringBuffer(str);
        }

        /**
         * Construct a new object using <code>buf</code> for internal
         * storage.  The contents of <code>buf</code> at the time of
         * construction are used as the initial contents.  <em>Note!
         * Modifications to <code>buf</code> will modify this object, and
         * vice versa.</em>
         * @param buf object to be used as internal storage
         * @stable ICU 2.0
         */
        public ReplaceableString(StringBuffer buf)
        {
            this.buf = buf;
        }

        /**
         * Construct a new empty object.
         * @stable ICU 2.0
         */
        public ReplaceableString()
        {
            buf = new StringBuffer();
        }

        /**
         * Return the contents of this object as a <code>String</code>.
         * @return string contents of this object
         * @stable ICU 2.0
         */
        public override string ToString()
        {
            return buf.ToString();
        }

        /**
         * Return a substring of the given string.
         * @stable ICU 2.0
         */
        public string Substring(int start, int count) // ICU4N NOTE: Using .NET semantics here - be vigilant about use
        {
            return buf.ToString(start, count);
        }

        /**
         * Return the number of characters contained in this object.
         * <code>Replaceable</code> API.
         * @stable ICU 2.0
         */
        public virtual int Length
        {
            get { return buf.Length; }
        }

        /**
         * Return the character at the given position in this object.
         * <code>Replaceable</code> API.
         * @param offset offset into the contents, from 0 to
         * <code>length()</code> - 1
         * @stable ICU 2.0
         */
        //    @Override
        //public char charAt(int offset)
        //    {
        //        return buf.charAt(offset);
        //    }
        public char this[int index]
        {
            get { return buf[index]; }
        }

        /**
         * Return the 32-bit code point at the given 16-bit offset into
         * the text.  This assumes the text is stored as 16-bit code units
         * with surrogate pairs intermixed.  If the offset of a leading or
         * trailing code unit of a surrogate pair is given, return the
         * code point of the surrogate pair.
         * @param offset an integer between 0 and <code>length()</code>-1
         * inclusive
         * @return 32-bit code point of text at given offset
         * @stable ICU 2.0
         */
        public virtual int Char32At(int offset)
        {
            return UTF16.CharAt(buf, offset);
        }

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
        //    @Override
        //public void getChars(int srcStart, int srcLimit, char[] dst, int dstStart)
        //    {
        //        if (srcStart != srcLimit)
        //        {
        //            buf.getChars(srcStart, srcLimit, dst, dstStart);
        //        }
        //    }
        public virtual void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
        {
            if (count > 0)
            {
                buf.CopyTo(sourceIndex, destination, destinationIndex, count);
            }
        }

        /**
         * Replace zero or more characters with new characters.
         * <code>Replaceable</code> API.
         * @param start the beginning index, inclusive; <code>0 &lt;= start
         * &lt;= limit</code>.
         * @param limit the ending index, exclusive; <code>start &lt;= limit
         * &lt;= length()</code>.
         * @param text new text to replace characters <code>start</code> to
         * <code>limit - 1</code>
         * @stable ICU 2.0
         */
        public virtual void Replace(int start, int limit, string text)
        {
            buf.Replace(start, limit, text);
        }

        /**
         * Replace a substring of this object with the given text.
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
        public virtual void Replace(int start, int limit, char[] chars,
                        int charsStart, int charsLen)
        {
            buf.Delete(start, limit);
            buf.Insert(start, chars, charsStart, charsLen);
        }

        /**
         * Copy a substring of this object, retaining attribute (out-of-band)
         * information.  This method is used to duplicate or reorder substrings.
         * The destination index must not overlap the source range.
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
        public virtual void Copy(int start, int limit, int dest)
        {
            if (start == limit && start >= 0 && start <= buf.Length)
            {
                return;
            }
            char[] text = new char[limit - start];
            //getChars(start, limit, text, 0);
            CopyTo(start, text, 0, limit - start);
            Replace(dest, dest, text, 0, limit - start);
        }

        /**
         * Implements Replaceable
         * @stable ICU 2.0
         */
        public virtual bool HasMetaData
        {
            get { return false; }
        }
    }
}
