using System;

namespace ICU4N.Util
{
    /// <summary>
    /// Exception thrown when the requested resource type 
    /// is not the same type as the available resource
    /// </summary>
    /// <author>ram</author>
    /// <stable>ICU 3.0</stable>
#if FEATURE_SERIALIZABLE_EXCEPTIONS
    [Serializable]
#endif
    public class UResourceTypeMismatchException : Exception
    {
        /// <summary>
        /// Constuct the exception with the given message
        /// </summary>
        /// <param name="message">The error message for this exception.</param>
        /// <stable>ICU 3.0</stable>
        public UResourceTypeMismatchException(string message)
            : base(message)
        {
        }

#if FEATURE_SERIALIZABLE_EXCEPTIONS
        /// <summary>
        /// Initializes a new instance of this class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected UResourceTypeMismatchException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}
