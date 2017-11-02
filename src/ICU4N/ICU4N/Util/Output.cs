using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Util
{
    /// <summary>
    /// Simple struct-like class for output parameters.
    /// </summary>
    /// <typeparam name="T">The type of the parameter.</typeparam>
    /// <stable>ICU 4.8</stable>
    public class Output<T>
    {
        /**
         * The value field
         * @stable ICU 4.8
         */
        public T Value { get; set; }

        /**
         * {@inheritDoc}
         * @stable ICU 4.8
         */
        public override string ToString()
        {
            return Value == null ? "null" : Value.ToString();
        }

        /**
         * Constructs an empty <c>Output</c>
         * @stable ICU 4.8
         */
        public Output()
        {
        }

        /**
         * Constructs an <c>Output</c> with the given value.
         * @param value the initial value
         * @stable ICU 4.8
         */
        public Output(T value)
        {
            this.Value = value;
        }
    }
}
