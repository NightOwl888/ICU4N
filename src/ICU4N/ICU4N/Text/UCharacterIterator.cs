using ICU4N.Impl;
using ICU4N.Support.Text;
using System;
using System.Text;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Text
{
    /// <summary>
    /// Abstract class that defines an API for iteration on text objects.This is an interface for forward and backward
    /// iteration and random access into a text object. Forward iteration is done with post-increment and backward iteration
    /// is done with pre-decrement semantics, while the <code>java.text.CharacterIterator</code> interface methods provided
    /// forward iteration with "pre-increment" and backward iteration with pre-decrement semantics. This API is more
    /// efficient for forward iteration over code points. The other major difference is that this API can do both code unit
    /// and code point iteration, <code>java.text.CharacterIterator</code> can only iterate over code units and is limited to
    /// BMP (0 - 0xFFFF)
    /// </summary>
    /// <author>Ram</author>
    /// <stable>ICU 2.4</stable>
    public abstract class UCharacterIterator : IUForwardCharacterIterator
#if FEATURE_CLONEABLE
        , ICloneable
#endif
    {
        // ICU4N specific - copy over the constants, since they are not automatically inherited
        public static readonly int DONE = UForwardCharacterIterator.DONE;

        /**
         * Protected default constructor for the subclasses
         *
         * @stable ICU 2.4
         */
        protected UCharacterIterator()
        {
        }

        // static final methods ----------------------------------------------------

        /**
         * Returns a <code>UCharacterIterator</code> object given a <code>Replaceable</code> object.
         *
         * @param source
         *            a valid source as a <code>Replaceable</code> object
         * @return UCharacterIterator object
         * @exception IllegalArgumentException
         *                if the argument is null
         * @stable ICU 2.4
         */
        public static UCharacterIterator GetInstance(IReplaceable source)
        {
            return new ReplaceableUCharacterIterator(source);
        }

        /**
         * Returns a <code>UCharacterIterator</code> object given a source string.
         *
         * @param source
         *            a string
         * @return UCharacterIterator object
         * @exception IllegalArgumentException
         *                if the argument is null
         * @stable ICU 2.4
         */
        public static UCharacterIterator GetInstance(string source)
        {
            return new ReplaceableUCharacterIterator(source);
        }

        /**
         * Returns a <code>UCharacterIterator</code> object given a source character array.
         *
         * @param source
         *            an array of UTF-16 code units
         * @return UCharacterIterator object
         * @exception IllegalArgumentException
         *                if the argument is null
         * @stable ICU 2.4
         */
        public static UCharacterIterator GetInstance(char[] source)
        {
            return GetInstance(source, 0, source.Length);
        }

        /**
         * Returns a <code>UCharacterIterator</code> object given a source character array.
         *
         * @param source
         *            an array of UTF-16 code units
         * @return UCharacterIterator object
         * @exception IllegalArgumentException
         *                if the argument is null
         * @stable ICU 2.4
         */
        public static UCharacterIterator GetInstance(char[] source, int start, int limit)
        {
            return new UCharArrayIterator(source, start, limit);
        }

        /**
         * Returns a <code>UCharacterIterator</code> object given a source StringBuffer.
         *
         * @param source
         *            an string buffer of UTF-16 code units
         * @return UCharacterIterator object
         * @exception IllegalArgumentException
         *                if the argument is null
         * @stable ICU 2.4
         */
        public static UCharacterIterator GetInstance(StringBuffer source)
        {
            return new ReplaceableUCharacterIterator(source);
        }

        /**
         * Returns a <code>UCharacterIterator</code> object given a CharacterIterator.
         *
         * @param source
         *            a valid CharacterIterator object.
         * @return UCharacterIterator object
         * @exception IllegalArgumentException
         *                if the argument is null
         * @stable ICU 2.4
         */
        public static UCharacterIterator GetInstance(CharacterIterator source)
        {
            return new CharacterIteratorWrapper(source);
        }

        // public methods ----------------------------------------------------------
        /**
         * Returns a <code>java.text.CharacterIterator</code> object for the underlying text of this iterator. The returned
         * iterator is independent of this iterator.
         *
         * @return java.text.CharacterIterator object
         * @stable ICU 2.4
         */
        public virtual CharacterIterator GetCharacterIterator()
        {
            return new UCharacterIteratorWrapper(this);
        }

        /**
         * Returns the code unit at the current index. If index is out of range, returns DONE. Index is not changed.
         *
         * @return current code unit
         * @stable ICU 2.4
         */
        public abstract int Current { get; }

        /**
         * Returns the codepoint at the current index. If the current index is invalid, DONE is returned. If the current
         * index points to a lead surrogate, and there is a following trail surrogate, then the code point is returned.
         * Otherwise, the code unit at index is returned. Index is not changed.
         *
         * @return current codepoint
         * @stable ICU 2.4
         */
        public virtual int CurrentCodePoint()
        {
            int ch = Current;
            if (UTF16.IsLeadSurrogate((char)ch))
            {
                // advance the index to get the
                // next code point
                Next();
                // due to post increment semantics
                // current() after next() actually
                // returns the char we want
                int ch2 = Current;
                // current should never change
                // the current index so back off
                Previous();

                if (UTF16.IsTrailSurrogate((char)ch2))
                {
                    // we found a surrogate pair
                    // return the codepoint
                    return Character.ToCodePoint((char)ch, (char)ch2);
                }
            }
            return ch;
        }

        /**
         * Returns the length of the text
         *
         * @return length of the text
         * @stable ICU 2.4
         */
        public abstract int Length { get; }

        /**
         * Gets the current index in text.
         *
         * @return current index in text.
         * @stable ICU 2.4
         */
        public abstract int Index { get; set; }

        /**
         * Returns the UTF16 code unit at index, and increments to the next code unit (post-increment semantics). If index
         * is out of range, DONE is returned, and the iterator is reset to the limit of the text.
         *
         * @return the next UTF16 code unit, or DONE if the index is at the limit of the text.
         * @stable ICU 2.4
         */
        public abstract int Next();

        /**
         * Returns the code point at index, and increments to the next code point (post-increment semantics). If index does
         * not point to a valid surrogate pair, the behavior is the same as <code>next()</code>. Otherwise the iterator is
         * incremented past the surrogate pair, and the code point represented by the pair is returned.
         *
         * @return the next codepoint in text, or DONE if the index is at the limit of the text.
         * @stable ICU 2.4
         */
        public virtual int NextCodePoint()
        {
            int ch1 = Next();
            if (UTF16.IsLeadSurrogate((char)ch1))
            {
                int ch2 = Next();
                if (UTF16.IsTrailSurrogate((char)ch2))
                {
                    return Character.ToCodePoint((char)ch1, (char)ch2);
                }
                else if (ch2 != DONE)
                {
                    // unmatched surrogate so back out
                    Previous();
                }
            }
            return ch1;
        }

        /**
         * Decrement to the position of the previous code unit in the text, and return it (pre-decrement semantics). If the
         * resulting index is less than 0, the index is reset to 0 and DONE is returned.
         *
         * @return the previous code unit in the text, or DONE if the new index is before the start of the text.
         * @stable ICU 2.4
         */
        public abstract int Previous();

        /**
         * Retreat to the start of the previous code point in the text, and return it (pre-decrement semantics). If the
         * index is not preceeded by a valid surrogate pair, the behavior is the same as <code>previous()</code>. Otherwise
         * the iterator is decremented to the start of the surrogate pair, and the code point represented by the pair is
         * returned.
         *
         * @return the previous code point in the text, or DONE if the new index is before the start of the text.
         * @stable ICU 2.4
         */
        public virtual int PreviousCodePoint()
        {
            int ch1 = Previous();
            if (UTF16.IsTrailSurrogate((char)ch1))
            {
                int ch2 = Previous();
                if (UTF16.IsLeadSurrogate((char)ch2))
                {
                    return Character.ToCodePoint((char)ch2, (char)ch1);
                }
                else if (ch2 != DONE)
                {
                    // unmatched trail surrogate so back out
                    Next();
                }
            }
            return ch1;
        }

        // ICU4N NOTE: Setter made into property
        ///**
        // * Sets the index to the specified index in the text.
        // *
        // * @param index
        // *            the index within the text.
        // * @exception IndexOutOfBoundsException
        // *                is thrown if an invalid index is supplied
        // * @stable ICU 2.4
        // */
        //public abstract void SetIndex(int index);

        /**
         * Sets the current index to the limit.
         *
         * @stable ICU 2.4
         */
        public virtual void SetToLimit()
        {
            Index = Length;
        }

        /**
         * Sets the current index to the start.
         *
         * @stable ICU 2.4
         */
        public virtual void SetToStart()
        {
            Index = 0;
        }

        /**
         * Fills the buffer with the underlying text storage of the iterator If the buffer capacity is not enough a
         * exception is thrown. The capacity of the fill in buffer should at least be equal to length of text in the
         * iterator obtained by calling <code>getLength()</code>). <b>Usage:</b>
         *
         * <pre>
         *         UChacterIterator iter = new UCharacterIterator.getInstance(text);
         *         char[] buf = new char[iter.getLength()];
         *         iter.getText(buf);
         *
         *         OR
         *         char[] buf= new char[1];
         *         int len = 0;
         *         for(;;){
         *             try{
         *                 len = iter.getText(buf);
         *                 break;
         *             }catch(IndexOutOfBoundsException e){
         *                 buf = new char[iter.getLength()];
         *             }
         *         }
         * </pre>
         *
         * @param fillIn
         *            an array of chars to fill with the underlying UTF-16 code units.
         * @param offset
         *            the position within the array to start putting the data.
         * @return the number of code units added to fillIn, as a convenience
         * @exception IndexOutOfBoundsException
         *                exception if there is not enough room after offset in the array, or if offset &lt; 0.
         * @stable ICU 2.4
         */
        public abstract int GetText(char[] fillIn, int offset);

        /**
         * Convenience override for <code>getText(char[], int)</code> that provides an offset of 0.
         *
         * @param fillIn
         *            an array of chars to fill with the underlying UTF-16 code units.
         * @return the number of code units added to fillIn, as a convenience
         * @exception IndexOutOfBoundsException
         *                exception if there is not enough room in the array.
         * @stable ICU 2.4
         */
        public int GetText(char[] fillIn)
        {
            return GetText(fillIn, 0);
        }

        /**
         * Convenience method for returning the underlying text storage as as string
         *
         * @return the underlying text storage in the iterator as a string
         * @stable ICU 2.4
         */
        public virtual string GetText()
        {
            char[] text = new char[Length];
            GetText(text);
            return new string(text);
        }

        /**
         * Moves the current position by the number of code units specified, either forward or backward depending on the
         * sign of delta (positive or negative respectively). If the resulting index would be less than zero, the index is
         * set to zero, and if the resulting index would be greater than limit, the index is set to limit.
         *
         * @param delta
         *            the number of code units to move the current index.
         * @return the new index.
         * @exception IndexOutOfBoundsException
         *                is thrown if an invalid index is supplied
         * @stable ICU 2.4
         *
         */
        public virtual int MoveIndex(int delta)
        {
            int x = Math.Max(0, Math.Min(Index + delta, Length));
            Index = x;
            return x;
        }

        /**
         * Moves the current position by the number of code points specified, either forward or backward depending on the
         * sign of delta (positive or negative respectively). If the current index is at a trail surrogate then the first
         * adjustment is by code unit, and the remaining adjustments are by code points. If the resulting index would be
         * less than zero, the index is set to zero, and if the resulting index would be greater than limit, the index is
         * set to limit.
         *
         * @param delta
         *            the number of code units to move the current index.
         * @return the new index
         * @exception IndexOutOfBoundsException
         *                is thrown if an invalid delta is supplied
         * @stable ICU 2.4
         */
        public virtual int MoveCodePointIndex(int delta)
        {
            if (delta > 0)
            {
                while (delta > 0 && NextCodePoint() != DONE)
                {
                    delta--;
                }
            }
            else
            {
                while (delta < 0 && PreviousCodePoint() != DONE)
                {
                    delta++;
                }
            }
            if (delta != 0)
            {
                throw new IndexOutOfRangeException();
            }

            return Index;
        }

        /**
         * Creates a copy of this iterator, independent from other iterators. If it is not possible to clone the iterator,
         * returns null.
         *
         * @return copy of this iterator
         * @stable ICU 2.4
         */

        public virtual object Clone()
        {
            return base.MemberwiseClone();
        }
    }
}
