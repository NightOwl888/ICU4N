using System;
using System.Collections.Generic;
using System.Globalization;

namespace ICU4N.Globalization
{
    /// <summary>
    /// Extensions to <see cref="CompareInfo"/>.
    /// </summary>
    public static class CompareInfoExtensions
    {
        /// <summary>
        /// Returns a <see cref="CompareInfo"/> wrapped in a class that implements <see cref="IComparer{String}"/>.
        /// </summary>
        /// <param name="compareInfo">A <see cref="CompareInfo"/> instance.</param>
        /// <returns>An <see cref="IComparer{String}"/> that uses the collation rules of <paramref name="compareInfo"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="compareInfo"/> is <c>null</c>.</exception>
        public static IComparer<string> AsComparer(this CompareInfo compareInfo)
        {
            return new CompareInfoComparer(compareInfo, CompareOptions.None);
        }

        /// <summary>
        /// Returns a <see cref="CompareInfo"/> wrapped in a class that implements <see cref="IComparer{String}"/>
        /// with the specfied <see cref="CompareOptions"/>.
        /// </summary>
        /// <param name="compareInfo">A <see cref="CompareInfo"/> instance.</param>
        /// <param name="options">A <see cref="CompareOptions"/> enum.</param>
        /// <returns>An <see cref="IComparer{String}"/> that uses the collation rules of <paramref name="compareInfo"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="compareInfo"/> is <c>null</c>.</exception>
        public static IComparer<string> AsComparer(this CompareInfo compareInfo, CompareOptions options)
        {
            return new CompareInfoComparer(compareInfo, options);
        }

        #region Nested Class: CompareInfoComparer

        private class CompareInfoComparer : IComparer<string>
        {
            private readonly CompareInfo compareInfo;
            private readonly CompareOptions options;

            public CompareInfoComparer(CompareInfo compareInfo, CompareOptions options)
            {
                this.compareInfo = compareInfo ?? throw new ArgumentNullException(nameof(compareInfo));
                this.options = options;
            }

            public int Compare(string string1, string string2)
            {
                return compareInfo.Compare(string1, string2, options);
            }
        }

        #endregion
    }
}
