using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ICU4N.Util
{
    [Obsolete("This API is ICU internal only.")]
    public class OutputInt 
    {
        /**
         * The value field.
         *
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        public int Value { get; set; }

        /**
         * Constructs an <code>OutputInt</code> with value 0.
         *
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        public OutputInt()
        {
        }

        /**
         * Constructs an <code>OutputInt</code> with the given value.
         *
         * @param value the initial value
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        public OutputInt(int value)
        {
            this.Value = value;
        }

        /**
         * {@inheritDoc}
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        public override string ToString()
        {
            return Value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
