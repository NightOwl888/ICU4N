using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Support.IO
{
    /// <summary>
    /// HeapByteBuffer, ReadWriteHeapByteBuffer and ReadOnlyHeapByteBuffer compose
    /// the implementation of array based byte buffers.
    /// <para/>
    /// ReadOnlyHeapByteBuffer extends HeapByteBuffer with all the write methods
    /// throwing read only exception.
    /// <para/>
    /// This class is sealed final for runtime performance.
    /// </summary>
    internal sealed class ReadOnlyHeapByteBuffer : HeapByteBuffer
    {
        internal static ReadOnlyHeapByteBuffer Copy(HeapByteBuffer other, int markOfOther)
        {
            ReadOnlyHeapByteBuffer buf = new ReadOnlyHeapByteBuffer(
                    other.backingArray, other.Capacity, other.offset);
            buf.limit = other.Limit;
            buf.position = other.Position;
            buf.mark = markOfOther;
            buf.SetOrder(other.Order);
            return buf;
        }

        internal ReadOnlyHeapByteBuffer(byte[] backingArray, int capacity, int arrayOffset)
            : base(backingArray, capacity, arrayOffset)
        {
        }


        public override ByteBuffer AsReadOnlyBuffer()
        {
            return Copy(this, mark);
        }


        public override ByteBuffer Compact()
        {
            throw new ReadOnlyBufferException();
        }

        public override ByteBuffer Duplicate()
        {
            return Copy(this, mark);
        }

        public override bool IsReadOnly
        {
            get { return true; }
        }


        protected override byte[] ProtectedArray
        {
            get { throw new ReadOnlyBufferException(); }
        }


        protected override int ProtectedArrayOffset
        {
            get { throw new ReadOnlyBufferException(); }
        }


        protected override bool ProtectedHasArray
        {
            get { return false; }
        }


        public override ByteBuffer Put(byte b)
        {
            throw new ReadOnlyBufferException();
        }


        public override ByteBuffer Put(int index, byte b)
        {
            throw new ReadOnlyBufferException();
        }


        public override ByteBuffer Put(byte[] src, int off, int len)
        {
            throw new ReadOnlyBufferException();
        }


        public override ByteBuffer PutDouble(double value)
        {
            throw new ReadOnlyBufferException();
        }


        public override ByteBuffer PutDouble(int index, double value)
        {
            throw new ReadOnlyBufferException();
        }


        public override ByteBuffer PutSingle(float value)
        {
            throw new ReadOnlyBufferException();
        }


        public override ByteBuffer PutSingle(int index, float value)
        {
            throw new ReadOnlyBufferException();
        }


        public override ByteBuffer PutInt32(int value)
        {
            throw new ReadOnlyBufferException();
        }


        public override ByteBuffer PutInt32(int index, int value)
        {
            throw new ReadOnlyBufferException();
        }


        public override ByteBuffer PutInt64(int index, long value)
        {
            throw new ReadOnlyBufferException();
        }


        public override ByteBuffer PutInt64(long value)
        {
            throw new ReadOnlyBufferException();
        }


        public override ByteBuffer PutInt16(int index, short value)
        {
            throw new ReadOnlyBufferException();
        }


        public override ByteBuffer PutInt16(short value)
        {
            throw new ReadOnlyBufferException();
        }


        public override ByteBuffer Put(ByteBuffer buf)
        {
            throw new ReadOnlyBufferException();
        }


        public override ByteBuffer Slice()
        {
            ReadOnlyHeapByteBuffer slice = new ReadOnlyHeapByteBuffer(backingArray,
                    Remaining, offset + position);
            slice.order = order;
            return slice;
        }
    }
}
