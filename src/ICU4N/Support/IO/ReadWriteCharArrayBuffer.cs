using System;
using System.Diagnostics.CodeAnalysis;

namespace ICU4N.Support.IO
{
    /// <summary>
    /// CharArrayBuffer, ReadWriteCharArrayBuffer and ReadOnlyCharArrayBuffer compose
    /// the implementation of array based char buffers.
    /// <para/>
    /// ReadWriteCharArrayBuffer extends CharArrayBuffer with all the write methods.
    /// <para/>
    /// This class is marked sealed for runtime performance.
    /// </summary>
    internal sealed class ReadWriteCharArrayBuffer : CharArrayBuffer
    {
        internal static ReadWriteCharArrayBuffer Copy(CharArrayBuffer other, int markOfOther)
        {
            ReadWriteCharArrayBuffer buf = new ReadWriteCharArrayBuffer(other
                    .Capacity, other.backingArray, other.offset);
            buf.limit = other.Limit;
            buf.position = other.Position;
            buf.mark = markOfOther;
            return buf;
        }

        internal ReadWriteCharArrayBuffer(char[] array)
            : base(array)
        {
        }

        internal ReadWriteCharArrayBuffer(int capacity)
            : base(capacity)
        {
        }

        internal ReadWriteCharArrayBuffer(int capacity, char[] backingArray, int arrayOffset)
            : base(capacity, backingArray, arrayOffset)
        {
        }

        public override CharBuffer AsReadOnlyBuffer()
        {
            return ReadOnlyCharArrayBuffer.Copy(this, mark);
        }

        public override CharBuffer Compact()
        {
            System.Array.Copy(backingArray, position + offset, backingArray, offset,
                    Remaining);
            position = limit - position;
            limit = capacity;
            mark = UnsetMark;
            return this;
        }

        public override CharBuffer Duplicate()
        {
            return Copy(this, mark);
        }

        public override bool IsReadOnly
        {
            get { return false; }
        }

        [SuppressMessage("Microsoft.Performance", "CA1819", Justification = "design requires some writable array properties")]
        protected override char[] ProtectedArray
        {
            get { return backingArray; }
        }

        protected override int ProtectedArrayOffset
        {
            get { return offset; }
        }

        protected override bool ProtectedHasArray
        {
            get { return true; }
        }

        public override CharBuffer Put(char c)
        {
            if (position == limit)
            {
                throw new BufferOverflowException();
            }
            backingArray[offset + position++] = c;
            return this;
        }

        public override CharBuffer Put(int index, char c)
        {
            if (index < 0 || index >= limit)
            {
                throw new IndexOutOfRangeException();
            }
            backingArray[offset + index] = c;
            return this;
        }

        public override CharBuffer Put(char[] src, int off, int len)
        {
            int length = src.Length;
            if (off < 0 || len < 0 || (long)len + (long)off > length)
            {
                throw new IndexOutOfRangeException();
            }
            if (len > Remaining)
            {
                throw new BufferOverflowException();
            }
            System.Array.Copy(src, off, backingArray, offset + position, len);
            position += len;
            return this;
        }

        public override CharBuffer Slice()
        {
            return new ReadWriteCharArrayBuffer(Remaining, backingArray, offset
                    + position);
        }

    }
}
