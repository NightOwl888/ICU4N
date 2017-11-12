using System;

namespace ICU4N.Support.IO
{
    /// <summary>
    /// <see cref="Int16ArrayBuffer"/>, <see cref="ReadWriteInt16ArrayBuffer"/> and <see cref="ReadOnlyInt16ArrayBuffer"/>
    /// compose the implementation of array based short buffers.
    /// <para/>
    /// <see cref="Int16ArrayBuffer"/> implements all the shared readonly methods and is extended
    /// by the other two classes.
    /// <para/>
    /// All methods are marked sealed for runtime performance.
    /// </summary>
    internal abstract class Int16ArrayBuffer : Int16Buffer
    {
        protected internal readonly short[] backingArray;

        protected internal readonly int offset;

        internal Int16ArrayBuffer(short[] array)
            : this(array.Length, array, 0)
        {
        }

        internal Int16ArrayBuffer(int capacity)
            : this(capacity, new short[capacity], 0)
        {
        }

        internal Int16ArrayBuffer(int capacity, short[] backingArray, int offset)
            : base(capacity)
        {
            this.backingArray = backingArray;
            this.offset = offset;
        }

        public override sealed short Get()
        {
            if (position == limit)
            {
                throw new BufferUnderflowException();
            }
            return backingArray[offset + position++];
        }

        public override sealed short Get(int index)
        {
            if (index < 0 || index >= limit)
            {
                throw new IndexOutOfRangeException();
            }
            return backingArray[offset + index];
        }

        public override sealed Int16Buffer Get(short[] dest, int off, int len)
        {
            int length = dest.Length;
            if (off < 0 || len < 0 || (long)off + (long)len > length)
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
