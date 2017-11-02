using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Text
{
    public interface IUForwardCharacterIterator
    {
        /**
         * Returns the UTF16 code unit at index, and increments to the next
         * code unit (post-increment semantics).  If index is out of
         * range, DONE is returned, and the iterator is reset to the limit
         * of the text.
         * @return the next UTF16 code unit, or DONE if the index is at the limit
         *         of the text.
         * @stable ICU 2.4  
         */
        int Next();

        /**
         * Returns the code point at index, and increments to the next code
         * point (post-increment semantics).  If index does not point to a
         * valid surrogate pair, the behavior is the same as
         * <code>next()</code>.  Otherwise the iterator is incremented past
         * the surrogate pair, and the code point represented by the pair
         * is returned.
         * @return the next codepoint in text, or DONE if the index is at
         *         the limit of the text.
         * @stable ICU 2.4  
         */
        int NextCodePoint();
    }

    public static class UForwardCharacterIterator
    {
        /**
         * Indicator that we have reached the ends of the UTF16 text.
         * @stable ICU 2.4
         */
        public static readonly int DONE = -1;
    }
}
