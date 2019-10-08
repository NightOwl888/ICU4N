namespace ICU4N.Support.IO
{
    /// <summary>
    /// <see cref="Int16ArrayBuffer"/>, <see cref="ReadWriteInt16ArrayBuffer"/> and <see cref="ReadOnlyInt16ArrayBuffer"/>
    /// compose the implementation of array based short buffers.
    /// <para/>
    /// <see cref="ReadOnlyInt16ArrayBuffer"/> extends <see cref="Int16ArrayBuffer"/> with all the write
    /// methods throwing read only exception.
    /// <para/>
    /// This class is marked sealed for runtime performance.
    /// </summary>
    internal sealed class ReadOnlyInt16ArrayBuffer : Int16ArrayBuffer
    {
        internal static ReadOnlyInt16ArrayBuffer Copy(Int16ArrayBuffer other, int markOfOther)
        {
            ReadOnlyInt16ArrayBuffer buf = new ReadOnlyInt16ArrayBuffer(other
                    .Capacity, other.backingArray, other.offset);
            buf.limit = other.Limit;
            buf.position = other.Position;
            buf.mark = markOfOther;
            return buf;
        }

        internal ReadOnlyInt16ArrayBuffer(int capacity, short[] backingArray, int arrayOffset)
            : base(capacity, backingArray, arrayOffset)
        {
        }

        public override Int16Buffer AsReadOnlyBuffer()
        {
            return Duplicate();
        }

        public override Int16Buffer Compact()
        {
            throw new ReadOnlyBufferException();
        }

        public override Int16Buffer Duplicate()
        {
            return Copy(this, mark);
        }

        public override bool IsReadOnly
        {
            get { return true; }
        }

        protected override short[] ProtectedArray
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

        public override Int16Buffer Put(Int16Buffer buf)
        {
            throw new ReadOnlyBufferException();
        }

        public override Int16Buffer Put(short c)
        {
            throw new ReadOnlyBufferException();
        }

        public override Int16Buffer Put(int index, short c)
        {
            throw new ReadOnlyBufferException();
        }

        public override sealed Int16Buffer Put(short[] src, int off, int len)
        {
            throw new ReadOnlyBufferException();
        }

        public override Int16Buffer Slice()
        {
            return new ReadOnlyInt16ArrayBuffer(Remaining, backingArray, offset
                    + position);
        }

    }
}
