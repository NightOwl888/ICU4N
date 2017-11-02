using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Impl
{
    public class IcuArgumentException : ArgumentException
    {
        public IcuArgumentException(string message)
            : base(message)
        {
        }

        public IcuArgumentException(Exception innerException)
            : base(string.Empty, innerException)
        {
        }

        public IcuArgumentException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
