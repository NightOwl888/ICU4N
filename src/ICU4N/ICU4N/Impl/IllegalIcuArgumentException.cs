using System;

namespace ICU4N.Impl
{
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
    }
}
