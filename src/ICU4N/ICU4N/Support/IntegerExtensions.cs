using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Support
{
    public static class IntegerExtensions
    {
        public static string ToHexString(this int codePoint)
        {
            return codePoint.ToString("x4");
        }
    }
}
