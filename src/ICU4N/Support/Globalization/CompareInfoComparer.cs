using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ICU4N.Support.Globalization
{
    public class CompareInfoComparer : IComparer<string>
    {
        private readonly CompareInfo compareInfo;

        public CompareInfoComparer(CompareInfo compareInfo)
        {
            if (compareInfo == null)
                throw new ArgumentNullException(nameof(compareInfo));
            this.compareInfo = compareInfo;
        }

        public int Compare(string string1, string string2)
        {
            return compareInfo.Compare(string1, string2);
        }
    }
}
