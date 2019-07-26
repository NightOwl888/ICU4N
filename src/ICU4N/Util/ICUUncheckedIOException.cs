using System;

namespace ICU4N.Util
{
    /// <summary>
    /// Unchecked version of <see cref="System.IO.IOException"/>.
    /// Some ICU APIs do not throw the standard exception but instead wrap it
    /// into this unchecked version.
    /// </summary>
    /// <stable>ICU 53</stable>
#if FEATURE_SERIALIZABLE_EXCEPTIONS
    [Serializable]
#endif
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

#if FEATURE_SERIALIZABLE_EXCEPTIONS
        /// <summary>
        /// Initializes a new instance of this class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected ICUUncheckedIOException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}
