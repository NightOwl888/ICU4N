using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace ICU4N.Support.IO
{
    /// <summary>
    /// <see cref="Int32ArrayBuffer"/>, <see cref="ReadWriteInt32ArrayBuffer"/> and <see cref="ReadOnlyInt32ArrayBuffer"/> compose
    /// the implementation of array based int buffers.
    /// <para/>
    /// <see cref="ReadWriteInt32ArrayBuffer"/> extends <see cref="Int32ArrayBuffer"/> with all the write methods.
    /// <para/>
    /// All methods are marked sealed for runtime performance.
    /// </summary>
    internal sealed class ReadWriteInt32ArrayBuffer : Int32ArrayBuffer
    {
        internal static ReadWriteInt32ArrayBuffer Copy(Int32ArrayBuffer other, int markOfOther)
        {
            ReadWriteInt32ArrayBuffer buf = new ReadWriteInt32ArrayBuffer(other
                    .Capacity, other.backingArray, other.offset);
            buf.limit = other.Limit;
            buf.position = other.Position;
            buf.mark = markOfOther;
            return buf;
        }

        internal ReadWriteInt32ArrayBuffer(int[] array)
                : base(array)
        {
        }

        internal ReadWriteInt32ArrayBuffer(int capacity)
            : base(capacity)
        {
        }

        internal ReadWriteInt32ArrayBuffer(int capacity, int[] backingArray, int arrayOffset)
            : base(capacity, backingArray, arrayOffset)
        {
        }

        public override Int32Buffer AsReadOnlyBuffer()
        {
            return ReadOnlyInt32ArrayBuffer.Copy(this, mark);
        }

        public override Int32Buffer Compact()
        {
            System.Array.Copy(backingArray, position + offset, backingArray, offset,
                    Remaining);
            position = limit - position;
            limit = capacity;
            mark = UNSET_MARK;
            return this;
        }

        public override Int32Buffer Duplicate()
        {
            return Copy(this, mark);
        }

        public override bool IsReadOnly
        {
            get { return false; }
        }

        [SuppressMessage("Microsoft.Performance", "CA1819", Justification = "design requires some writable array properties")]
        protected override int[] ProtectedArray
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

        public override Int32Buffer Put(int c)
        {
            if (position == limit)
            {
                throw new BufferOverflowException();
            }
            backingArray[offset + position++] = c;
            return this;
        }

        public override Int32Buffer Put(int index, int c)
        {
            if (index < 0 || index >= limit)
            {
                throw new IndexOutOfRangeException();
            }
            backingArray[offset + index] = c;
            return this;
        }

        public override Int32Buffer Put(int[] src, int off, int len)
        {
            int length = src.Length;
            if (off < 0 || len < 0 || (long)off + (long)len > length)
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

        public override Int32Buffer Slice()
        {
            return new ReadWriteInt32ArrayBuffer(Remaining, backingArray, offset
                    + position);
        }
    }
}
