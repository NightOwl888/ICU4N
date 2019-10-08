using ICU4N.Support.Text;
using System;

namespace ICU4N.Support.IO
{
    /// <summary>
    /// CharArrayBuffer, ReadWriteCharArrayBuffer and ReadOnlyCharArrayBuffer compose
    /// the implementation of array based char buffers.
    /// <para/>
    /// CharArrayBuffer implements all the shared readonly methods and is extended by
    /// the other two classes.
    /// <para/>
    /// All methods are marked sealed for runtime performance.
    /// </summary>
    internal abstract class CharArrayBuffer : CharBuffer
    {
        protected internal readonly char[] backingArray;

        protected internal readonly int offset;

        internal CharArrayBuffer(char[] array)
            : this(array.Length, array, 0)
        {
        }

        internal CharArrayBuffer(int capacity)
            : this(capacity, new char[capacity], 0)
        {
        }

        internal CharArrayBuffer(int capacity, char[] backingArray, int offset)
            : base(capacity)
        {
            this.backingArray = backingArray;
            this.offset = offset;
        }

        public override sealed char Get()
        {
            if (position == limit)
            {
                throw new BufferUnderflowException();
            }
            return backingArray[offset + position++];
        }

        public override sealed char Get(int index)
        {
            if (index < 0 || index >= limit)
            {
                throw new IndexOutOfRangeException();
            }
            return backingArray[offset + index];
        }

        public override sealed CharBuffer Get(char[] dest, int off, int len)
        {
            int length = dest.Length;
            if ((off < 0) || (len < 0) || (long)off + (long)len > length)
            {
                throw new IndexOutOfRangeException();
            }
            if (len > Remaining)
            {
                throw new BufferUnderflowException();
            }
            System.Array.Copy(backingArray, offset + position, dest, off, len);
            position += len;
            return this;
        }

        public override sealed bool IsDirect
        {
            get { return false; }
        }

        public override sealed ByteOrder Order
        {
            get { return ByteOrder.NativeOrder; }
        }

        public override sealed ICharSequence SubSequence(int start, int end)
        {
            if (start < 0 || end < start || end > Remaining)
            {
                throw new IndexOutOfRangeException();
            }

            CharBuffer result = Duplicate();
            result.Limit = position + end;
            result.Position = position + start;
            return result;
        }

        public override sealed string ToString()
        {
            return new string(backingArray, offset + position, Remaining);
        }
    }
}
