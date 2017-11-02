using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Util
{
    /// <summary>
    /// Base class for unchecked, ICU-specific exceptions.
    /// </summary>
    /// <stable>ICU 53</stable>
    public class ICUException : Exception
    {
        /**
         * Default constructor.
         *
         * @stable ICU 53
         */
        public ICUException()
        {
        }

        /**
         * Constructor.
         *
         * @param message exception message string
         * @stable ICU 53
         */
        public ICUException(string message)
            : base(message)
        {
        }

        /**
         * Constructor.
         *
         * @param cause original exception
         * @stable ICU 53
         */
        public ICUException(Exception innerException)
            : base(innerException.Message, innerException)
        {
        }

        /**
         * Constructor.
         *
         * @param message exception message string
         * @param cause original exception
         * @stable ICU 53
         */
        public ICUException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
