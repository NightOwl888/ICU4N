using System;
using System.Collections.Generic;
using System.Text;

namespace System.Globalization
{
    public static class SortKeyExtensions
    {
        /// <summary>
        /// Compare this <see cref="SortKey"/> to another <see cref="SortKey"/>.  The
        /// collation rules of the Collator that created this key are
        /// applied.
        /// </summary>
        /// <remarks>
        /// <strong>Note:</strong> Comparison between <see cref="SortKey"/>s
        /// created by different <see cref="Collator"/>s might return incorrect
        /// results. See class documentation.
        /// </remarks>
        /// <param name="sortKey">This <see cref="SortKey"/>.</param>
        /// <param name="target">Target <see cref="SortKey"/>.</param>
        /// <returns>
        /// An integer value.  If the value is less than zero this <see cref="SortKey"/>
        /// is less than than target, if the value is zero they are equal, and
        /// if the value is greater than zero this <see cref="SortKey"/> is greater
        /// than target.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="target"/> is <c>null</c>.</exception>
        /// <seealso cref="ICU4N.Text.Collator.Compare(string, string)"/>
        /// <stable>ICU 2.8</stable>
        public static int CompareTo(this SortKey sortKey, SortKey target)
        {
            if (sortKey == null)
                throw new ArgumentNullException(nameof(sortKey));
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            for (int i = 0; ; ++i)
            {
                int l = sortKey.KeyData[i] & 0xff;
                int r = target.KeyData[i] & 0xff;
                if (l < r)
                {
                    return -1;
                }
                else if (l > r)
                {
                    return 1;
                }
                else if (l == 0)
                {
                    return 0;
                }
            }
        }

        public static SortKey Merge(this SortKey sortKey, SortKey source)
        {
            throw new NotImplementedException();
        }
    }
}

