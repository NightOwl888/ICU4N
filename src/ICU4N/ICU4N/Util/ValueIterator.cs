using System.Collections.Generic;

namespace ICU4N.Util
{
    /// <summary>
    /// The return result container of each iteration. Stores the next
    /// integer index and its associated value Object.
    /// </summary>
    /// <remarks>
    /// NOTE: This is equivelent to ValueIterator.Element in ICU4J.
    /// </remarks>
    /// <stable>ICU 2.6</stable>
    public sealed class ValueEnumeratorElement
    {
        // public data members ----------------------------------------

        /// <summary>
        /// Integer index of the current iteration.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public int Integer { get; set; }

        /// <summary>
        /// Gets the <see cref="object"/> value associated with the integer index.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public object Value { get; set; }

        // public constructor ------------------------------------------

        /// <summary>
        /// Empty default constructor.
        /// </summary>
        /// <stable>ICU 2.4</stable>
        public ValueEnumeratorElement()
        {
        }
    }

    /// <summary>
    /// Interface for enabling iteration over sets of &lt;int, object&gt;, where
    /// int is the sorted integer index in ascending order, and object its
    /// associated value.
    /// </summary>
    /// <remarks>
    /// The <see cref="IValueEnumerator"/> allows iterations over integer indexes in the range
    /// of <see cref="int.MinValue"/> to <see cref="int.MaxValue"/> inclusive. Implementations of
    /// <see cref="IValueEnumerator"/> should specify their own maximum subrange within the above
    /// range that is meaningful to its applications.
    /// <para/>
    /// Most implementations will be created by factory methods, such as the
    /// character name iterator in <see cref="Lang.UCharacter.GetNameEnumerator()"/>. See example below.
    /// <para/>
    /// Example of use:
    /// <code>
    /// IValueEnumerator iterator = UCharacter.GetNameIterator();
    /// iterator.SetRange(UCharacter.MIN_VALUE, UCharacter.MAX_VALUE);
    /// while (iterator.MoveNext())
    /// {
    ///     Console.WriteLine("Codepoint \\u" +
    ///                         iterator.Current.Integer.ToHexString() +
    ///                         " has the character name " + (string)iterator.Current.Value);
    /// }
    /// </code>
    /// <para/>
    /// NOTE: This is equivalent to ValueIterator in ICU4J.
    /// </remarks>
    /// <author>synwee</author>
    /// <stable>ICU 2.6</stable>
    public interface IValueEnumerator : IEnumerator<ValueEnumeratorElement>
    {
        // public methods -------------------------------------------------

        /// <summary>
        /// Returns the next result for this iteration and returns
        /// true if we are not at the end of the iteration, false otherwise.
        /// </summary>
        /// <returns>true if we are not at the end of the iteration, false otherwise.</returns>
        /// <seealso cref="ValueEnumeratorElement"/>
        /// <stable>ICU 2.6</stable>
        new bool MoveNext();

        /// <summary>
        /// Resets the iterator to start iterating from the integer index
        /// <see cref="int.MinValue"/> or X if a <c>SetRange(X, Y)</c> has been called previously.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        new void Reset();

        /// <summary>
        /// Restricts the range of integers to iterate and resets the iteration
        /// to begin at the index argument start.
        /// </summary>
        /// <remarks>
        /// If <see cref="SetRange(int, int)"/> is not performed before <see cref="MoveNext()"/> is
        /// called, the iteration will start from the integer index
        /// <see cref="int.MinValue"/> and end at <see cref="int.MaxValue"/>.
        /// <para/>
        /// If this range is set outside the meaningful range specified by the
        /// implementation, <see cref="MoveNext()"/> will always return false.
        /// </remarks>
        /// <param name="start">First integer in the range to iterate.</param>
        /// <param name="limit">One more than the last integer in the range.</param>
        /// <exception cref="ArgumentException">Thrown when attempting to set an
        /// illegal range. E.g limit &lt;= start.</exception>
        /// <stable>ICU 2.6</stable>
        void SetRange(int start, int limit);
    }
}
