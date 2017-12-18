using System;

namespace ICU4N.Util
{
    /// <summary>
    /// Thrown by methods in <see cref="ULocale"/> and <see cref="ULocale.Builder"/> to
    /// indicate that an argument is not a well-formed BCP 47 tag.
    /// </summary>
    /// <seealso cref="ULocale"/>
    /// <stable>ICU 4.2</stable>
    public class IllformedLocaleException : Exception // ICU4N TODO: API Subclass FormatException?
    {
        //private static readonly long serialVersionUID = 1L;

        private int _errIdx = -1;

        /// <summary>
        /// Constructs a new <see cref="IllformedLocaleException"/> with no
        /// detail message and -1 as the error index.
        /// </summary>
        /// <stable>ICU 4.6</stable>
        public IllformedLocaleException()
            : base()
        {
        }

        /// <summary>
        /// Constructs a new <see cref="IllformedLocaleException"/> with the
        /// given message and -1 as the error index.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <stable>ICU 4.2</stable>
        public IllformedLocaleException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="IllformedLocaleException"/> with the
        /// given message, inner exception and -1 as the error index.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The original exception.</param>
        /// <stable>ICU 4.2</stable>
        public IllformedLocaleException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="IllformedLocaleException"/> with the
        /// given message and the error index.  The error index is the approximate
        /// offset from the start of the ill-formed value to the point where the
        /// parse first detected an error.  A negative error index value indicates
        /// either the error index is not applicable or unknown.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="errorIndex">The index.</param>
        /// <stable>ICU 4.2</stable>
        public IllformedLocaleException(string message, int errorIndex)
            : base(message + ((errorIndex < 0) ? "" : " [at index " + errorIndex + "]"))
        {
            _errIdx = errorIndex;
        }

        /// <summary>
        /// Returns the index where the error was found. A negative value indicates
        /// either the error index is not applicable or unknown.
        /// </summary>
        /// <returns>the error index</returns>
        /// <stable>ICU 4.2</stable>
        public virtual int ErrorIndex
        {
            get { return _errIdx; }
        }
    }
}
