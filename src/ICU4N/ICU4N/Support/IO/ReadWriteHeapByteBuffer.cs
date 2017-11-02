using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Support.IO
{
    /// <summary>
    /// HeapByteBuffer, ReadWriteHeapByteBuffer and ReadOnlyHeapByteBuffer compose
    /// the implementation of array based byte buffers.
    /// <para/>
    /// ReadWriteHeapByteBuffer extends HeapByteBuffer with all the write methods.
    /// <para/>
    /// This class is marked sealed for runtime performance.
    /// </summary>
    internal sealed class ReadWriteHeapByteBuffer : HeapByteBuffer
    {
        internal static ReadWriteHeapByteBuffer Copy(HeapByteBuffer other, int markOfOther)
        {
            ReadWriteHeapByteBuffer buf = new ReadWriteHeapByteBuffer(
                    other.backingArray, other.Capacity, other.offset);
            buf.limit = other.Limit;
            buf.position = other.Position;
            buf.mark = markOfOther;
            buf.SetOrder(other.Order);
            return buf;
        }

        internal ReadWriteHeapByteBuffer(byte[] backingArray)
            : base(backingArray)
        {
        }

        internal ReadWriteHeapByteBuffer(int capacity)
            : base(capacity)
        {
        }

        internal ReadWriteHeapByteBuffer(byte[] backingArray, int capacity, int arrayOffset)
            : base(backingArray, capacity, arrayOffset)
        {
        }

        public override ByteBuffer AsReadOnlyBuffer()
        {
            return ReadOnlyHeapByteBuffer.Copy(this, mark);
        }

        public override ByteBuffer Compact()
        {
            System.Array.Copy(backingArray, position + offset, backingArray, offset,
                    Remaining);
            position = limit - position;
            limit = capacity;
            mark = UNSET_MARK;
            return this;
        }

        public override ByteBuffer Duplicate()
        {
            return Copy(this, mark);
        }

        public override bool IsReadOnly
        {
            get { return false; }
        }

        protected override byte[] ProtectedArray
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

        public override ByteBuffer Put(byte b)
        {
            if (position == limit)
            {
                throw new BufferOverflowException();
            }
            backingArray[offset + position++] = b;
            return this;
        }

        public override ByteBuffer Put(int index, byte b)
        {
            if (index < 0 || index >= limit)
            {
                throw new IndexOutOfRangeException();
            }
            backingArray[offset + index] = b;
            return this;
        }

        /*
         * Override ByteBuffer.put(byte[], int, int) to improve performance.
         * 
         * (non-Javadoc)
         * 
         * @see java.nio.ByteBuffer#put(byte[], int, int)
         */

        public override ByteBuffer Put(byte[] src, int off, int len)
        {
            if (off < 0 || len < 0 || (long)off + (long)len > src.Length)
            {
                throw new IndexOutOfRangeException();
            }
            if (len > Remaining)
            {
                throw new BufferOverflowException();
            }
            if (IsReadOnly)
            {
                throw new ReadOnlyBufferException();
            }
            System.Array.Copy(src, off, backingArray, offset + position, len);
            position += len;
            return this;
        }

        public override ByteBuffer PutDouble(double value)
        {
            return PutInt64(Number.DoubleToRawInt64Bits(value));
        }

        public override ByteBuffer PutDouble(int index, double value)
        {
            return PutInt64(index, Number.DoubleToRawInt64Bits(value));
        }

        public override ByteBuffer PutSingle(float value)
        {
            return PutInt32(Number.SingleToInt32Bits(value));
        }

        public override ByteBuffer PutSingle(int index, float value)
        {
            return PutInt32(index, Number.SingleToInt32Bits(value));
        }

        public override ByteBuffer PutInt32(int value)
        {
            int newPosition = position + 4;
            if (newPosition > limit)
            {
                throw new BufferOverflowException();
            }
            Store(position, value);
            position = newPosition;
            return this;
        }

        public override ByteBuffer PutInt32(int index, int value)
        {
            if (index < 0 || (long)index + 4 > limit)
            {
                throw new IndexOutOfRangeException();
            }
            Store(index, value);
            return this;
        }

        public override ByteBuffer PutInt64(int index, long value)
        {
            if (index < 0 || (long)index + 8 > limit)
            {
                throw new IndexOutOfRangeException();
            }
            Store(index, value);
            return this;
        }

        public override ByteBuffer PutInt64(long value)
        {
            int newPosition = position + 8;
            if (newPosition > limit)
            {
                throw new BufferOverflowException();
            }
            Store(position, value);
            position = newPosition;
            return this;
        }

        public override ByteBuffer PutInt16(int index, short value)
        {
            if (index < 0 || (long)index + 2 > limit)
            {
                throw new IndexOutOfRangeException();
            }
            Store(index, value);
            return this;
        }

        public override ByteBuffer PutInt16(short value)
        {
            int newPosition = position + 2;
            if (newPosition > limit)
            {
                throw new BufferOverflowException();
            }
            Store(position, value);
            position = newPosition;
            return this;
        }

        public override ByteBuffer Slice()
        {
            ReadWriteHeapByteBuffer slice = new ReadWriteHeapByteBuffer(
                    backingArray, Remaining, offset + position);
            slice.order = order;
            return slice;
        }
    }
}
