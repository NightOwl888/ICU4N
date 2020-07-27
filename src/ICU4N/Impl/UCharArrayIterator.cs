using ICU4N.Text;
using System;

namespace ICU4N.Impl
{
    /// <author>Doug Felt</author>
    public sealed class UCharArrayIterator : UCharacterIterator
    {
        private readonly char[] text;
        private readonly int start;
        private readonly int limit;
        private int pos;

        public UCharArrayIterator(char[] text, int start, int limit)
        {
            if (start < 0 || limit > text.Length || start > limit)
            {
                throw new ArgumentException("start: " + start + " or limit: "
                                                   + limit + " out of range [0, "
                                                   + text.Length + ")");
            }
            this.text = text;
            this.start = start;
            this.limit = limit;

            this.pos = start;
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
            int len = limit - start;
            if (offset + len > fillIn.Length)
            {
                // ICU4N specific - IndexOutOfRangeException expected
                // when the array length is exceeded, but Array.Copy throws
                // ArgumentException in this case.
                throw new IndexOutOfRangeException();
            }
            System.Array.Copy(text, start, fillIn, offset, len);
            return len;
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
