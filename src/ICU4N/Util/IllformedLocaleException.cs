using ICU4N.Globalization;
using System;

namespace ICU4N.Util
{
    /// <summary>
    /// Thrown by methods in <see cref="UCultureInfo"/> and <see cref="UCultureInfoBuilder"/> to
    /// indicate that an argument is not a well-formed BCP 47 tag.
    /// </summary>
    /// <seealso cref="UCultureInfo"/>
    /// <stable>ICU 4.2</stable>
#if FEATURE_SERIALIZABLE_EXCEPTIONS
    [Serializable]
#endif
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

#if FEATURE_SERIALIZABLE_EXCEPTIONS
        /// <summary>
        /// Initializes a new instance of this class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected IllformedLocaleException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
#endif

        /// <summary>
        /// Returns the index where the error was found. A negative value indicates
        /// either the error index is not applicable or unknown.
        /// </summary>
        /// <returns>the error index</returns>
        /// <stable>ICU 4.2</stable>
        public virtual int ErrorIndex => _errIdx;
    }
}
