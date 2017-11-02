using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Util
{
    /// <summary>
    /// Unchecked version of <see cref="System.IO.IOException"/>.
    /// Some ICU APIs do not throw the standard exception but instead wrap it
    /// into this unchecked version.
    /// </summary>
    public class ICUUncheckedIOException : Exception
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ICUUncheckedIOException()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">exception message string</param>
        public ICUUncheckedIOException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="innerException">original exception</param>
        public ICUUncheckedIOException(Exception innerException)
            : base(string.Empty, innerException)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">exception message string</param>
        /// <param name="innerException">original exception</param>
        public ICUUncheckedIOException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
