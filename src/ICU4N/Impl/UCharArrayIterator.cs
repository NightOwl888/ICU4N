using ICU4N.Text;
using System;

namespace ICU4N.Impl
{
    /// <author>Doug Felt</author>
    internal sealed class UCharArrayIterator : UCharacterIterator // ICU4N TODO: API Changed from public to internal until this can be converted into an enumerator
    {
        private readonly char[] text;
        private readonly int start;
        private readonly int limit;
        private int pos;

        public UCharArrayIterator(char[] text, int startIndex, int length) // ICU4N: Changed limit to length
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (length < 0 || length > text.Length)
                throw new ArgumentOutOfRangeException(nameof(length));
            if (length > text.Length - startIndex)
                throw new ArgumentOutOfRangeException(string.Empty, $"{nameof(startIndex)}: {startIndex} + {nameof(length)}: {length} > {nameof(text.Length)}: {text.Length}");

            this.text = text;
            this.start = startIndex;
            this.limit = startIndex + length;

            this.pos = startIndex;
        }

        public override int Current => pos < limit ? text[pos] : Done;

        public override int Length => limit - start;

        public override int Index
        {
            get => pos - start;
            set
            {
                if (value < 0 || value > limit - start)
                {
                    throw new IndexOutOfRangeException("index: " + value +
                                                        " out of range [0, "
                                                        + (limit - start) + ")");
                }
                pos = start + value;
            }
        }

        public override int Next()
        {
            return pos < limit ? text[pos++] : Done;
        }

        public override int Previous()
        {
            return pos > start ? text[--pos] : Done;
        }


        public override int GetText(char[] fillIn, int offset)
        {
            // ICU4N: Added guard clauses that were missing in Java
            if (fillIn == null)
                throw new ArgumentNullException(nameof(fillIn));
            int length = limit - start;
            if (offset < 0 || offset > length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (offset + length > fillIn.Length)
                throw new ArgumentException($"Not enough space in the destination array: {length}", nameof(fillIn));

            System.Array.Copy(text, start, fillIn, offset, length);
            return length;
        }

        /// <summary>
        /// Creates a copy of this iterator, does not clone the underlying
        /// <see cref="IReplaceable"/> object
        /// </summary>
        /// <returns>copy of this iterator</returns>
        public override object Clone()
        {
            return base.Clone();
        }
    }
}
