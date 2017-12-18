using System;

namespace ICU4N.Util
{
    /// <summary>
    /// Unchecked version of <see cref="System.IO.IOException"/>.
    /// Some ICU APIs do not throw the standard exception but instead wrap it
    /// into this unchecked version.
    /// </summary>
    /// <stable>ICU 53</stable>
    public class ICUUncheckedIOException : Exception
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <stable>ICU 53</stable>
        public ICUUncheckedIOException()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">exception message string</param>
        /// <stable>ICU 53</stable>
        public ICUUncheckedIOException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="innerException">original exception</param>
        /// <stable>ICU 53</stable>
        public ICUUncheckedIOException(Exception innerException)
            : base(string.Empty, innerException)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">exception message string</param>
        /// <param name="innerException">original exception</param>
        /// <stable>ICU 53</stable>
        public ICUUncheckedIOException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
