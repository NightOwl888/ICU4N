using System.Collections.Generic;

namespace ICU4N.Util
{
    /// <summary>
    /// Return result wrapper for <see cref="IRangeValueEnumerator"/>.
    /// Stores the start and limit of the continous result range and the
    /// common value all integers between [start, limit - 1] has.
    /// </summary>
    /// <remarks>
    /// This is equivalent to RangeValueIterator.Element in ICU4J.
    /// </remarks>
    /// <stable>ICU 2.6</stable>
    public class RangeValueEnumeratorElement
    {
        // public data member ---------------------------------------------

        /// <summary>
        /// Starting integer of the continuous result range that has the same
        /// value.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public int Start { get; set; }
        /// <summary>
        /// (End + 1) integer of continuous result range that has the same
        /// value.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public int Limit { get; set; }
        /// <summary>
        /// Gets the common value of the continous result range.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public int Value { get; set; }

        // public constructor --------------------------------------------

        /// <summary>
        /// Empty default constructor.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public RangeValueEnumeratorElement()
        {
        }
    }

    /// <summary>
    /// Interface for enabling iteration over sets of &lt;int index, int value&gt;,
    /// where index is the sorted integer index in ascending order and value, its
    /// associated integer value.
    /// </summary>
    /// <remarks>
    /// The result for each iteration is the consecutive range of
    /// &lt;int index, int value&gt; with the same value. Result is represented by
    /// &lt;start, limit, value&gt; where
    /// <list type="bullet">
    ///     <item><description>start is the starting integer of the result range</description></item>
    ///     <item><description>
    ///         limit is 1 after the maximum integer that follows start, such that
    ///         all integers between start and (limit - 1), inclusive, have the same
    ///         associated integer value.
    ///     </description></item>
    ///     <item><description>
    ///         value is the integer value that all integers from start to (limit - 1)
    ///         share in common.
    ///     </description></item>
    /// </list>
    /// <para/>
    /// Hence value(start) = value(start + 1) = .... = value(start + n) = .... =
    /// value(limit - 1). However value(start -1) != value(start) and
    /// value(limit) != value(start).
    /// <para/>
    /// Most implementations will be created by factory methods, such as the
    /// character type iterator in <see cref="UChar.GetTypeEnumerator()"/>. See example below.
    /// <para/>
    /// This is equivalent to RangeValueIterator in ICU4J.
    /// </remarks>
    public interface IRangeValueEnumerator : IEnumerator<RangeValueEnumeratorElement>
    {
        // public methods -------------------------------------------------

        /// <summary>
        /// Gets the current <see cref="RangeValueEnumeratorElement"/> in the iteration.
        /// </summary>
        new RangeValueEnumeratorElement Current { get; }

        /// <summary>
        /// Returns the next maximal result range with a common value and returns
        /// true if we are not at the end of the iteration, false otherwise.
        /// </summary>
        /// <returns>true if we are not at the end of the iteration, false otherwise.</returns>
        /// <seealso cref="RangeValueEnumeratorElement"/>
        /// <stable>ICU 2.6</stable>
        new bool MoveNext();

        /// <summary>
        /// Resets the iterator to the beginning of the iteration.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        new void Reset();
    }
}
