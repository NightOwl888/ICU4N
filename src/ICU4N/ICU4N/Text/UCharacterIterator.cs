using ICU4N.Impl;
using ICU4N.Support.Text;
using System;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Text
{
    /// <summary>
    /// Abstract class that defines an API for iteration on text objects.This is an interface for forward and backward
    /// iteration and random access into a text object. Forward iteration is done with post-increment and backward iteration
    /// is done with pre-decrement semantics, while the <see cref="CharacterIterator"/> interface methods provided
    /// forward iteration with "pre-increment" and backward iteration with pre-decrement semantics. This API is more
    /// efficient for forward iteration over code points. The other major difference is that this API can do both code unit
    /// and code point iteration, <see cref="CharacterIterator"/> can only iterate over code units and is limited to
    /// BMP (0 - 0xFFFF)
    /// </summary>
    /// <author>Ram</author>
    /// <stable>ICU 2.4</stable>
    public abstract class UCharacterIterator : IUForwardCharacterIterator // ICU4N TODO: API Make into enumerator ?
#if FEATURE_CLONEABLE
        , ICloneable
#endif
    {
        /// <summary>
        /// Indicator that we have reached the ends of the UTF16 text.
        /// </summary>
        /// <draft>ICU4N 60</draft>
        // ICU4N specific - copy over the constants, since they are not automatically inherited
        public static readonly int Done = UForwardCharacterIterator.Done;

        /// <summary>
        /// Protected default constructor for the subclasses.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        protected UCharacterIterator()
        {
        }

        // static final methods ----------------------------------------------------

        /// <summary>
        /// Returns a <see cref="UCharacterIterator"/> object given a <see cref="IReplaceable"/> object.
        /// </summary>
        /// <param name="source">A valid source as a <see cref="IReplaceable"/> object.</param>
        /// <returns><see cref="UCharacterIterator"/> object.</returns>
        /// <exception cref="ArgumentException">If the argument is null.</exception>
        /// <stable>ICU 2.4</stable>
        public static UCharacterIterator GetInstance(IReplaceable source)
        {
            return new ReplaceableUCharacterIterator(source);
        }

        /// <summary>
        /// Returns a <see cref="UCharacterIterator"/> object given a source string.
        /// </summary>
        /// <param name="source">A string.</param>
        /// <returns><see cref="UCharacterIterator"/> object.</returns>
        /// <exception cref="ArgumentException">If the argument is null.</exception>
        /// <stable>ICU 2.4</stable>
        public static UCharacterIterator GetInstance(string source)
        {
            return new ReplaceableUCharacterIterator(source);
        }

        /// <summary>
        /// Returns a <see cref="UCharacterIterator"/> object given a source character array.
        /// </summary>
        /// <param name="source">An array of UTF-16 code units.</param>
        /// <returns><see cref="UCharacterIterator"/> object.</returns>
        /// <exception cref="ArgumentException">If the argument is null.</exception>
        /// <stable>ICU 2.4</stable>
        public static UCharacterIterator GetInstance(char[] source)
        {
            return GetInstance(source, 0, source.Length);
        }

        /// <summary>
        /// Returns a <see cref="UCharacterIterator"/> object given a source character array.
        /// </summary>
        /// <param name="source">An array of UTF-16 code units.</param>
        /// <param name="start"></param>
        /// <param name="limit"></param>
        /// <returns><see cref="UCharacterIterator"/> object.</returns>
        /// <exception cref="ArgumentException">If the argument is null.</exception>
        /// <stable>ICU 2.4</stable>
        public static UCharacterIterator GetInstance(char[] source, int start, int limit)
        {
            return new UCharArrayIterator(source, start, limit);
        }

        /// <summary>
        /// Returns a <see cref="UCharacterIterator"/> object given a source <see cref="StringBuffer"/>.
        /// </summary>
        /// <param name="source">A string buffer of UTF-16 code units.</param>
        /// <returns><see cref="UCharacterIterator"/> object.</returns>
        /// <exception cref="ArgumentException">If the argument is null.</exception>
        /// <stable>ICU 2.4</stable>
        public static UCharacterIterator GetInstance(StringBuffer source)
        {
            return new ReplaceableUCharacterIterator(source);
        }

        /// <summary>
        /// Returns a <see cref="UCharacterIterator"/> object given a <see cref="CharacterIterator"/>.
        /// </summary>
        /// <param name="source">A valid <see cref="CharacterIterator"/> object.</param>
        /// <returns><see cref="UCharacterIterator"/> object.</returns>
        /// <exception cref="ArgumentException">If the argument is null.</exception>
        /// <stable>ICU 2.4</stable>
        public static UCharacterIterator GetInstance(CharacterIterator source)
        {
            return new CharacterIteratorWrapper(source);
        }

        // public methods ----------------------------------------------------------

        /// <summary>
        /// Returns a <see cref="CharacterIterator"/> object for the underlying text of this iterator. The returned
        /// iterator is independent of this iterator.
        /// </summary>
        /// <returns><see cref="CharacterIterator"/> object.</returns>
        /// <stable>ICU 2.4</stable>
        public virtual CharacterIterator GetCharacterIterator()
        {
            return new UCharacterIteratorWrapper(this);
        }

        /// <summary>
        /// Returns the code unit at the current index. If index is out of range, returns <see cref="UForwardCharacterIterator.Done"/>. Index is not changed.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public abstract int Current { get; }

        /// <summary>
        /// Returns the codepoint at the current index. If the current index is invalid, <see cref="UForwardCharacterIterator.Done"/> is returned. If the current
        /// index points to a lead surrogate, and there is a following trail surrogate, then the code point is returned.
        /// Otherwise, the code unit at index is returned. Index is not changed.
        /// </summary>
        /// <returns>Current codepoint.</returns>
        /// <stable>ICU 2.4</stable>
        public virtual int CurrentCodePoint
        {
            get
            {
                int ch = Current;
                if (UTF16.IsLeadSurrogate((char)ch))
                {
                    // advance the index to get the
                    // next code point
                    MoveNext();
                    // due to post increment semantics
                    // current() after next() actually
                    // returns the char we want
                    int ch2 = Current;
                    // current should never change
                    // the current index so back off
                    MovePrevious();

                    if (UTF16.IsTrailSurrogate((char)ch2))
                    {
                        // we found a surrogate pair
                        // return the codepoint
                        return Character.ToCodePoint((char)ch, (char)ch2);
                    }
                }
                return ch;
            }
        }

        /// <summary>
        /// Returns the length of the text.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public abstract int Length { get; }

        /// <summary>
        /// Gets or Sets the current index in text.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">Is thrown if an invalid index is supplied.</exception>
        /// <stable>ICU 2.4</stable>
        public abstract int Index { get; set; }

        /// <summary>
        /// Returns the UTF16 code unit at index, and increments to the next code unit (post-increment semantics). If index
        /// is out of range, <see cref="UForwardCharacterIterator.Done"/> is returned, and the iterator is reset to the limit of the text.
        /// </summary>
        /// <returns>The next UTF16 code unit, or <see cref="UForwardCharacterIterator.Done"/> if the index is at the limit of the text.</returns>
        /// <stable>ICU 2.4</stable>
        public abstract int MoveNext();

        /// <summary>
        /// Returns the code point at index, and increments to the next code point (post-increment semantics). If index does
        /// not point to a valid surrogate pair, the behavior is the same as <see cref="MoveNext()"/>. Otherwise the iterator is
        /// incremented past the surrogate pair, and the code point represented by the pair is returned.
        /// </summary>
        /// <returns>The next codepoint in text, or <see cref="UForwardCharacterIterator.Done"/> if the index is at the limit of the text.</returns>
        /// <stable>ICU 2.4</stable>
        public virtual int MoveNextCodePoint()
        {
            int ch1 = MoveNext();
            if (UTF16.IsLeadSurrogate((char)ch1))
            {
                int ch2 = MoveNext();
                if (UTF16.IsTrailSurrogate((char)ch2))
                {
                    return Character.ToCodePoint((char)ch1, (char)ch2);
                }
                else if (ch2 != Done)
                {
                    // unmatched surrogate so back out
                    MovePrevious();
                }
            }
            return ch1;
        }

        /// <summary>
        /// Decrement to the position of the previous code unit in the text, and return it (pre-decrement semantics). If the
        /// resulting index is less than 0, the index is reset to 0 and <see cref="UForwardCharacterIterator.Done"/> is returned.
        /// </summary>
        /// <returns>The previous code unit in the text, or <see cref="UForwardCharacterIterator.Done"/> if the new index is before the start of the text.</returns>
        /// <stable>ICU 2.4</stable>
        public abstract int MovePrevious();

        /// <summary>
        /// Retreat to the start of the previous code point in the text, and return it (pre-decrement semantics). If the
        /// index is not preceeded by a valid surrogate pair, the behavior is the same as <see cref="MovePrevious()"/>. Otherwise
        /// the iterator is decremented to the start of the surrogate pair, and the code point represented by the pair is
        /// returned.
        /// </summary>
        /// <returns>The previous code point in the text, or <see cref="UForwardCharacterIterator.Done"/> if the new index is before the start of the text.</returns>
        /// <stable>ICU 2.4</stable>
        public virtual int MovePreviousCodePoint()
        {
            int ch1 = MovePrevious();
            if (UTF16.IsTrailSurrogate((char)ch1))
            {
                int ch2 = MovePrevious();
                if (UTF16.IsLeadSurrogate((char)ch2))
                {
                    return Character.ToCodePoint((char)ch2, (char)ch1);
                }
                else if (ch2 != Done)
                {
                    // unmatched trail surrogate so back out
                    MoveNext();
                }
            }
            return ch1;
        }

        /// <summary>
        /// Sets the current index to the limit.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public virtual void SetToLimit()
        {
            Index = Length;
        }

        /// <summary>
        /// Sets the current index to the start.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public virtual void SetToStart()
        {
            Index = 0;
        }

        /// <summary>
        /// Fills the buffer with the underlying text storage of the iterator. If the buffer capacity is not enough a
        /// exception is thrown. The capacity of the fill in buffer should at least be equal to length of text in the
        /// iterator obtained by getting <see cref="Length"/>.
        /// </summary>
        /// <remarks>
        /// <b>Usage:</b>
        /// <code>
        /// UChacterIterator iter = new UCharacterIterator.GetInstance(text);
        /// char[] buf = new char[iter.Length];
        /// iter.GetText(buf);
        /// </code>
        /// OR
        /// <code>
        /// char[] buf= new char[1];
        /// int len = 0;
        /// while (true)
        /// {
        ///     try
        ///     {
        ///         len = iter.GetText(buf);
        ///         break;
        ///     }
        ///     catch (IndexOutOfRangeException)
        ///     {
        ///         buf = new char[iter.Length];
        ///     }
        /// }
        /// </code>
        /// </remarks>
        /// <param name="fillIn">An array of chars to fill with the underlying UTF-16 code units.</param>
        /// <param name="offset">The position within the array to start putting the data.</param>
        /// <returns>The number of code units added to fillIn, as a convenience.</returns>
        /// <exception cref="IndexOutOfRangeException">Exception if there is not enough room after offset in the array, or if offset &lt; 0.</exception>
        /// <stable>ICU 2.4</stable>
        public abstract int GetText(char[] fillIn, int offset); // ICU4N TODO: API - try to work out how to check for an invalid state rather than rely on exception to be thrown

        /// <summary>
        /// Convenience override for <see cref="GetText(char[], int)"/> that provides an offset of 0.
        /// </summary>
        /// <param name="fillIn">An array of chars to fill with the underlying UTF-16 code units.</param>
        /// <returns>The number of code units added to fillIn, as a convenience.</returns>
        /// <exception cref="IndexOutOfRangeException">If there is not enough room in the array.</exception>
        /// <stable>ICU 2.4</stable>
        public int GetText(char[] fillIn)
        {
            return GetText(fillIn, 0);
        }

        /// <summary>
        /// Convenience method for returning the underlying text storage as as string.
        /// </summary>
        /// <returns>The underlying text storage in the iterator as a string.</returns>
        /// <stable>ICU 2.4</stable>
        public virtual string GetText()
        {
            char[] text = new char[Length];
            GetText(text);
            return new string(text);
        }

        /// <summary>
        /// Moves the current position by the number of code units specified, either forward or backward depending on the
        /// sign of delta (positive or negative respectively). If the resulting index would be less than zero, the index is
        /// set to zero, and if the resulting index would be greater than limit, the index is set to limit.
        /// </summary>
        /// <param name="delta">The number of code units to move the current index.</param>
        /// <returns>The new index.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown if an invalid index is supplied.</exception>
        /// <stable>ICU 2.4</stable>
        public virtual int MoveIndex(int delta)
        {
            int x = Math.Max(0, Math.Min(Index + delta, Length));
            Index = x;
            return x;
        }

        /// <summary>
        /// Moves the current position by the number of code points specified, either forward or backward depending on the
        /// sign of delta (positive or negative respectively). If the current index is at a trail surrogate then the first
        /// adjustment is by code unit, and the remaining adjustments are by code points. If the resulting index would be
        /// less than zero, the index is set to zero, and if the resulting index would be greater than limit, the index is
        /// set to limit.
        /// </summary>
        /// <param name="delta">The number of code units to move the current index.</param>
        /// <returns>The new index.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown if an invalid index is supplied.</exception>
        /// <stable>ICU 2.4</stable>
        public virtual int MoveCodePointIndex(int delta)
        {
            if (delta > 0)
            {
                while (delta > 0 && MoveNextCodePoint() != Done)
                {
                    delta--;
                }
            }
            else
            {
                while (delta < 0 && MovePreviousCodePoint() != Done)
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

        /// <summary>
        /// Creates a copy of this iterator, independent from other iterators. If it is not possible to clone the iterator,
        /// returns null.
        /// </summary>
        /// <returns>Copy of this iterator.</returns>
        /// <stable>ICU 2.4</stable>
        public virtual object Clone()
        {
            return base.MemberwiseClone();
        }
    }
}
