using System;

namespace ICU4N.Support.IO
{
    /// <summary>
    /// <see cref="Int32ArrayBuffer"/>, <see cref="ReadWriteInt32ArrayBuffer"/> and <see cref="ReadOnlyInt32ArrayBuffer"/> compose
    /// the implementation of array based int buffers.
    /// <para/>
    /// <see cref="Int32ArrayBuffer"/> implements all the shared readonly methods and is extended by
    /// the other two classes.
    /// <para/>
    /// All methods are marked sealed for runtime performance.
    /// </summary>
    internal abstract class Int32ArrayBuffer : Int32Buffer
    {
        protected internal readonly int[] backingArray;

        protected internal readonly int offset;

        internal Int32ArrayBuffer(int[] array)
                : this(array.Length, array, 0)
        {
        }

        internal Int32ArrayBuffer(int capacity)
                : this(capacity, new int[capacity], 0)
        {
        }

        internal Int32ArrayBuffer(int capacity, int[] backingArray, int offset)
                : base(capacity)
        {
            this.backingArray = backingArray;
            this.offset = offset;
        }

        public override sealed int Get()
        {
            if (position == limit)
            {
                throw new BufferUnderflowException();
            }
            return backingArray[offset + position++];
        }

        public override sealed int Get(int index)
        {
            if (index < 0 || index >= limit)
            {
                throw new IndexOutOfRangeException();
            }
            return backingArray[offset + index];
        }

        public override sealed Int32Buffer Get(int[] dest, int off, int len)
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
