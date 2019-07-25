using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Support
{
    internal static class ExceptionExtensions
    {
        public static void PrintStackTrace(this Exception exception)
        {
            Console.Out.WriteLine(exception.ToString());
        }
    }
}
