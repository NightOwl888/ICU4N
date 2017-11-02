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

        public override int Current
        {
            get { return pos < limit ? text[pos] : DONE; }
        }

        public override int Length
        {
            get { return limit - start; }
        }

        public override int Index
        {
            get { return pos - start; }
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
            return pos < limit ? text[pos++] : DONE;
        }

        public override int Previous()
        {
            return pos > start ? text[--pos] : DONE;
        }


        public override int GetText(char[] fillIn, int offset)
        {
            int len = limit - start;
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
