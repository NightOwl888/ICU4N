using System;

namespace ICU4N.Util
{
    /// <summary>
    /// Base class for unchecked, ICU-specific exceptions.
    /// </summary>
    /// <stable>ICU 53</stable>
    public class ICUException : Exception
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <stable>ICU 53</stable>
        public ICUException()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Exception message strings</param>
        /// <stable>ICU 53</stable>
        public ICUException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="innerException">Original exception.</param>
        /// <stable>ICU 53</stable>
        public ICUException(Exception innerException)
            : base(innerException.Message, innerException)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Exception message string.</param>
        /// <param name="innerException">Original exception.</param>
        /// <stable>ICU 53</stable>
        public ICUException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
