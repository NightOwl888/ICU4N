using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Util
{
    public class RangeValueIteratorElement
    {
        // public data member ---------------------------------------------

        /**
        * Starting integer of the continuous result range that has the same
        * value
        * @stable ICU 2.6
        */
        public int Start { get; set; }
        /**
        * (End + 1) integer of continuous result range that has the same
        * value
        * @stable ICU 2.6
        */
        public int Limit { get; set; }
        /**
        * Gets the common value of the continous result range
        * @stable ICU 2.6
        */
        public int Value { get; set; }

        // public constructor --------------------------------------------

        /**
         * Empty default constructor to make javadoc happy
         * @stable ICU 2.4
         */
        public RangeValueIteratorElement()
        {
        }
    }

    public interface IRangeValueIterator // ICU4N TODO: API - Refactor to enumerator
    {
        // public methods -------------------------------------------------

        /**
        * <p>Returns the next maximal result range with a common value and returns
        * true if we are not at the end of the iteration, false otherwise.
        * <p>If this returns a false, the contents of elements will not
        * be updated.
        * @param element for storing the result range and value
        * @return true if we are not at the end of the iteration, false otherwise.
        * @see Element
        * @stable ICU 2.6
        */
        bool Next(RangeValueIteratorElement element);

        /**
        * Resets the iterator to the beginning of the iteration.
        * @stable ICU 2.6
        */
        void Reset();
    }
}
