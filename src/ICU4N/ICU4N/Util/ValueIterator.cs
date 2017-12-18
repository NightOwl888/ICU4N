using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Util
{
    /// <summary>
    /// The return result container of each iteration. Stores the next
    /// integer index and its associated value Object.
    /// </summary>
    /// <stable>ICU 2.6</stable>
    public sealed class ValueIteratorElement
    {
        // public data members ----------------------------------------

        /**
        * Integer index of the current iteration
        * @stable ICU 2.6
        */
        public int Integer { get; set; }
        /**
        * Gets the Object value associated with the integer index.
        * @stable ICU 2.6
        */
        public object Value { get; set; }

        // public constructor ------------------------------------------

        /**
         * Empty default constructor to make javadoc happy
         * @stable ICU 2.4
         */
        public ValueIteratorElement()
        {
        }
    }

    public interface IValueIterator // ICU4N TODO: API - Refactor to enumerator
    {
        // public methods -------------------------------------------------

        /**
        * <p>Returns the next result for this iteration and returns
        * true if we are not at the end of the iteration, false otherwise.
        * <p>If this returns a false, the contents of elements will not
        * be updated.
        * @param element for storing the result index and value
        * @return true if we are not at the end of the iteration, false otherwise.
        * @see Element
        * @stable ICU 2.6
        */
        bool Next(ValueIteratorElement element);

        /**
        * <p>Resets the iterator to start iterating from the integer index
        * Integer.MIN_VALUE or X if a setRange(X, Y) has been called previously.
        *
        * @stable ICU 2.6
        */
        void Reset();

        /**
         * <p>Restricts the range of integers to iterate and resets the iteration
         * to begin at the index argument start.
         * <p>If setRange(start, end) is not performed before next(element) is
         * called, the iteration will start from the integer index
         * Integer.MIN_VALUE and end at Integer.MAX_VALUE.
         * <p>
         * If this range is set outside the meaningful range specified by the
         * implementation, next(element) will always return false.
         *
         * @param start first integer in the range to iterate
         * @param limit one more than the last integer in the range
         * @exception IllegalArgumentException thrown when attempting to set an
         *            illegal range. E.g limit &lt;= start
         * @stable ICU 2.6
         */
        void SetRange(int start, int limit);
    }
}
