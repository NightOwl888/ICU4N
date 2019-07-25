using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Support
{
    public static class CharExtensions
    {
        public static string ToHexString(this char chr)
        {
            return ((int)chr).ToString("x4");
        }
    }
}
