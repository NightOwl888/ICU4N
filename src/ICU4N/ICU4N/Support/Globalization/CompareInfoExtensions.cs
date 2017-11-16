using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ICU4N.Support.Globalization
{
    public static class CompareInfoExtensions
    {
        public static IComparer<string> ToComparer(this CompareInfo compareInfo)
        {
            return new CompareInfoComparer(compareInfo);
        }
    }
}
