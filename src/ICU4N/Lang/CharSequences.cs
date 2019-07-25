using System;

namespace ICU4N.Lang
{
    [Obsolete("This API is ICU internal only.")]
    public partial class CharSequences
    {
        // TODO
        // compareTo(a, b);
        // compareToIgnoreCase(a, b)
        // contentEquals(a, b)
        // contentEqualsIgnoreCase(a, b)

        // contains(a, b) => indexOf >= 0
        // endsWith(a, b)
        // startsWith(a, b)

        // lastIndexOf(a, b, fromIndex)
        // indexOf(a, ch, fromIndex)
        // lastIndexOf(a, ch, fromIndex);

        // s.trim() => UnicodeSet.trim(CharSequence s); return a subsequence starting with the first character not in the set to the last character not in the set.
        // add UnicodeSet.split(CharSequence s);

        // ICU4N specfic - moved all methods to CharSequencesExtension.tt

        /// <summary>
        /// Utility function for comparing objects that may be null
        /// string.
        /// </summary>
        [Obsolete("This API is ICU internal only.")]
        public static bool Equals<T>(T a, T b) where T : class
        {
            return a == null ? b == null
                    : b == null ? false
                            : a.Equals(b);
        }

        private CharSequences()
        {
        }
    }
}
