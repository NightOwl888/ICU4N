using System;

namespace ICU4N.Support.Text
{
    internal static class StringExtensions
    {
        public static int CompareToOrdinalIgnoreCase(this string str, string value)
        {
            return StringComparer.OrdinalIgnoreCase.Compare(str, value);
        }
    }
}
