using System;

namespace ICU4N.Util
{
    /// <summary>
    /// Exception thrown when the requested resource type 
    /// is not the same type as the available resource
    /// </summary>
    /// <author>ram</author>
    /// <stable>ICU 3.0</stable>
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
    }
}
