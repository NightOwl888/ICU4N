using System;

namespace ICU4N.Impl
{
#if FEATURE_SERIALIZABLE_EXCEPTIONS
    [Serializable]
#endif
    public class IcuArgumentException : ArgumentException
    {
        public IcuArgumentException(string message)
            : base(message)
        {
        }

        public IcuArgumentException(Exception innerException)
            : base(innerException.Message, innerException)
        {
        }

        public IcuArgumentException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

#if FEATURE_SERIALIZABLE_EXCEPTIONS
        /// <summary>
        /// Initializes a new instance of this class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected IcuArgumentException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}
