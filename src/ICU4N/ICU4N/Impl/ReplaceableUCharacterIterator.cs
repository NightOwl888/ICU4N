using ICU4N.Support.Text;
using ICU4N.Text;
using System;
using System.Collections.Generic;
using System.Text;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Impl
{
    // DLF docs must define behavior when Replaceable is mutated underneath
    // the iterator.
    // 
    // This and ICUCharacterIterator share some code, maybe they should share
    // an implementation, or the common state and implementation should be
    // moved up into UCharacterIterator.
    // 
    // What are first, last, and getBeginIndex doing here?!?!?!
    public class ReplaceableUCharacterIterator : UCharacterIterator
    {
        // public constructor ------------------------------------------------------

        /**
         * Public constructor
         * @param replaceable text which the iterator will be based on
         */
        public ReplaceableUCharacterIterator(IReplaceable replaceable)
        {
            if (replaceable == null)
            {
                throw new ArgumentException();
            }
            this.replaceable = replaceable;
            this.currentIndex = 0;
        }

        /**
         * Public constructor
         * @param str text which the iterator will be based on
         */
        public ReplaceableUCharacterIterator(string str)
        {
            if (str == null)
            {
                throw new ArgumentException();
            }
            this.replaceable = new ReplaceableString(str);
            this.currentIndex = 0;
        }

        /**
         * Public constructor
         * @param buf buffer of text on which the iterator will be based
         */
        public ReplaceableUCharacterIterator(StringBuffer buf)
        {
            if (buf == null)
            {
                throw new ArgumentException();
            }
            this.replaceable = new ReplaceableString(buf);
            this.currentIndex = 0;
        }

        // public methods ----------------------------------------------------------

        /**
         * Creates a copy of this iterator, does not clone the underlying
         * <code>Replaceable</code>object
         * @return copy of this iterator
         */
        public override object Clone()
        {
            return base.MemberwiseClone();
        }

        /**
         * Returns the current UTF16 character.
         * @return current UTF16 character
         */
        public override int Current
        {
            get
            {
                if (currentIndex < replaceable.Length)
                {
                    return replaceable[currentIndex];
                }
                return UForwardCharacterIterator.DONE;
            }
        }

        /**
         * Returns the current codepoint
         * @return current codepoint
         */
        public override int CurrentCodePoint()
        {
            // cannot use charAt due to it different
            // behaviour when index is pointing at a
            // trail surrogate, check for surrogates

            int ch = Current;
            if (UTF16.IsLeadSurrogate((char)ch))
            {
                // advance the index to get the next code point
                Next();
                // due to post increment semantics current() after next()
                // actually returns the next char which is what we want
                int ch2 = Current;
                // current should never change the current index so back off
                Previous();

                if (UTF16.IsTrailSurrogate((char)ch2))
                {
                    // we found a surrogate pair
                    return Character.ToCodePoint((char)ch, (char)ch2);
                }
            }
            return ch;
        }

        /**
         * Returns the length of the text
         * @return length of the text
         */
        public override int Length
        {
            get { return replaceable.Length; }
        }

        /**
         * Gets the current currentIndex in text.
         * @return current currentIndex in text.
         */
        public override int Index
        {
            get { return currentIndex; }
            set
            {
                if (value < 0 || value > replaceable.Length)
                {
                    throw new IndexOutOfRangeException();
                }
                this.currentIndex = value;
            }
        }

        /**
         * Returns next UTF16 character and increments the iterator's currentIndex by 1.
         * If the resulting currentIndex is greater or equal to the text length, the
         * currentIndex is reset to the text length and a value of DONECODEPOINT is
         * returned.
         * @return next UTF16 character in text or DONE if the new currentIndex is off the
         *         end of the text range.
         */
        public override int Next()
        {
            if (currentIndex < replaceable.Length)
            {
                return replaceable[currentIndex++];
            }
            return UForwardCharacterIterator.DONE;
        }


        /**
         * Returns previous UTF16 character and decrements the iterator's currentIndex by
         * 1.
         * If the resulting currentIndex is less than 0, the currentIndex is reset to 0 and a
         * value of DONECODEPOINT is returned.
         * @return next UTF16 character in text or DONE if the new currentIndex is off the
         *         start of the text range.
         */
        public override int Previous()
        {
            if (currentIndex > 0)
            {
                return replaceable[--currentIndex];
            }
            return UForwardCharacterIterator.DONE;
        }

        //    /**
        //     * <p>Sets the currentIndex to the specified currentIndex in the text and returns that
        //     * single UTF16 character at currentIndex.
        //     * This assumes the text is stored as 16-bit code units.</p>
        //     * @param currentIndex the currentIndex within the text.
        //     * @exception IllegalArgumentException is thrown if an invalid currentIndex is
        //     *            supplied. i.e. currentIndex is out of bounds.
        //     * @returns the character at the specified currentIndex or DONE if the specified
        //     *         currentIndex is equal to the end of the text.
        //     */

        //public override void setIndex(int currentIndex) 
        //    {
        //    if (currentIndex< 0 || currentIndex> replaceable.Length) {
        //        throw new IndexOutOfRangeException();
        //}
        //    this.currentIndex = currentIndex;
        //}


        public override int GetText(char[] fillIn, int offset)
        {
            int length = replaceable.Length;
            if (offset < 0 || offset + length > fillIn.Length)
            {
                throw new IndexOutOfRangeException(length.ToString());
            }
            //replaceable.GetChars(0, length, fillIn, offset);
            replaceable.CopyTo(0, fillIn, offset, length);
            return length;
        }

        // private data members ----------------------------------------------------

        /**
         * Replacable object
         */
        private IReplaceable replaceable;
        /**
         * Current currentIndex
         */
        private int currentIndex;

    }
}
