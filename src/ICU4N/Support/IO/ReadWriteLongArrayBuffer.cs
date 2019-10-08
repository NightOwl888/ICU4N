using System;

namespace ICU4N.Support.IO
{
    /// <summary>
    /// LongArrayBuffer, ReadWriteLongArrayBuffer and ReadOnlyLongArrayBuffer compose
    /// the implementation of array based long buffers.
    /// <para/>
    /// ReadWriteLongArrayBuffer extends LongArrayBuffer with all the write methods.
    /// <para/>
    /// This class is marked final for runtime performance.
    /// </summary>
    internal sealed class ReadWriteInt64ArrayBuffer : Int64ArrayBuffer
    {
        internal static ReadWriteInt64ArrayBuffer Copy(Int64ArrayBuffer other, int markOfOther)
        {
            ReadWriteInt64ArrayBuffer buf = new ReadWriteInt64ArrayBuffer(other
                    .Capacity, other.backingArray, other.offset);
            buf.limit = other.Limit;
            buf.position = other.Position;
            buf.mark = markOfOther;
            return buf;
        }

        internal ReadWriteInt64ArrayBuffer(long[] array)
            : base(array)
        {
        }

        internal ReadWriteInt64ArrayBuffer(int capacity)
            : base(capacity)
        {
        }

        internal ReadWriteInt64ArrayBuffer(int capacity, long[] backingArray, int arrayOffset)
            : base(capacity, backingArray, arrayOffset)
        {
        }


        public override Int64Buffer AsReadOnlyBuffer()
        {
            throw new NotImplementedException();
            //return ReadOnlyLongArrayBuffer.copy(this, mark);
        }

        public override Int64Buffer Compact()
        {
            System.Array.Copy(backingArray, position + offset, backingArray, offset,
                    Remaining);
            position = limit - position;
            limit = capacity;
            mark = UnsetMark;
            return this;
        }

        public override Int64Buffer Duplicate()
        {
            return Copy(this, mark);
        }

        public override bool IsReadOnly
        {
            get { return false; }
        }

        protected override long[] ProtectedArray
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

        public override Int64Buffer Put(long c)
        {
            if (position == limit)
            {
                throw new BufferOverflowException();
            }
            backingArray[offset + position++] = c;
            return this;
        }

        public override Int64Buffer Put(int index, long c)
        {
            if (index < 0 || index >= limit)
            {
                throw new IndexOutOfRangeException();
            }
            backingArray[offset + index] = c;
            return this;
        }

        public override Int64Buffer Put(long[] src, int off, int len)
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

        public override Int64Buffer Slice()
        {
            return new ReadWriteInt64ArrayBuffer(Remaining, backingArray, offset
                    + position);
        }
    }
}
