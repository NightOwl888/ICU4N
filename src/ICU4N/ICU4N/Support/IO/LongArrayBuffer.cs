using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Support.IO
{
    /// <summary>
    /// LongArrayBuffer, ReadWriteLongArrayBuffer and ReadOnlyLongArrayBuffer compose
    /// the implementation of array based long buffers.
    /// <para/>
    /// LongArrayBuffer implements all the shared readonly methods and is extended by
    /// the other two classes.
    /// <para/>
    /// All methods are marked final for runtime performance.
    /// </summary>
    internal abstract class Int64ArrayBuffer : Int64Buffer
    {
        protected internal readonly long[] backingArray;

        protected internal readonly int offset;

        internal Int64ArrayBuffer(long[] array)
                : this(array.Length, array, 0)
        {
        }

        internal Int64ArrayBuffer(int capacity)
                : this(capacity, new long[capacity], 0)
        {
        }

        internal Int64ArrayBuffer(int capacity, long[] backingArray, int offset)
            : base(capacity)
        {
            this.backingArray = backingArray;
            this.offset = offset;
        }


        public override sealed long Get()
        {
            if (position == limit)
            {
                throw new BufferUnderflowException();
            }
            return backingArray[offset + position++];
        }


        public override sealed long Get(int index)
        {
            if (index < 0 || index >= limit)
            {
                throw new IndexOutOfRangeException();
            }
            return backingArray[offset + index];
        }


        public override sealed Int64Buffer Get(long[] dest, int off, int len)
        {
            int length = dest.Length;
            if (off < 0 || len < 0 || (long)len + (long)off > length)
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
    }
}
