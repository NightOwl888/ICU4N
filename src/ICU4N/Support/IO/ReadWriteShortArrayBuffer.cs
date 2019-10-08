using System;
using System.Diagnostics.CodeAnalysis;

namespace ICU4N.Support.IO
{
    /// <summary>
    /// <see cref="Int16ArrayBuffer"/>, <see cref="ReadWriteInt16ArrayBuffer"/> and <see cref="ReadOnlyInt16ArrayBuffer"/>
    /// compose the implementation of array based short buffers.
    /// <para/>
    /// <see cref="ReadWriteInt16ArrayBuffer"/> extends <see cref="Int16ArrayBuffer"/> with all the write
    /// methods.
    /// <para/>
    /// This class is marked sealed for runtime performance.
    /// </summary>
    internal sealed class ReadWriteInt16ArrayBuffer : Int16ArrayBuffer
    {
        internal static ReadWriteInt16ArrayBuffer Copy(Int16ArrayBuffer other,
            int markOfOther)
        {
            ReadWriteInt16ArrayBuffer buf = new ReadWriteInt16ArrayBuffer(other
                    .Capacity, other.backingArray, other.offset);
            buf.limit = other.Limit;
            buf.position = other.Position;
            buf.mark = markOfOther;
            return buf;
        }

        internal ReadWriteInt16ArrayBuffer(short[] array)
            : base(array)
        {
        }

        internal ReadWriteInt16ArrayBuffer(int capacity)
            : base(capacity)
        {
        }

        internal ReadWriteInt16ArrayBuffer(int capacity, short[] backingArray,
                int arrayOffset)
            : base(capacity, backingArray, arrayOffset)
        {
        }

        public override Int16Buffer AsReadOnlyBuffer()
        {
            return ReadOnlyInt16ArrayBuffer.Copy(this, mark);
        }

        public override Int16Buffer Compact()
        {
            System.Array.Copy(backingArray, position + offset, backingArray, offset,
                    Remaining);
            position = limit - position;
            limit = capacity;
            mark = UnsetMark;
            return this;
        }

        public override Int16Buffer Duplicate()
        {
            return Copy(this, mark);
        }

        public override bool IsReadOnly
        {
            get { return false; }
        }

        [SuppressMessage("Microsoft.Performance", "CA1819", Justification = "design requires some writable array properties")]
        protected override short[] ProtectedArray
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

        public override Int16Buffer Put(short c)
        {
            if (position == limit)
            {
                throw new BufferOverflowException();
            }
            backingArray[offset + position++] = c;
            return this;
        }

        public override Int16Buffer Put(int index, short c)
        {
            if (index < 0 || index >= limit)
            {
                throw new IndexOutOfRangeException();
            }
            backingArray[offset + index] = c;
            return this;
        }

        public override Int16Buffer Put(short[] src, int off, int len)
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

        public override Int16Buffer Slice()
        {
            return new ReadWriteInt16ArrayBuffer(Remaining, backingArray, offset
                    + position);
        }

    }
}
