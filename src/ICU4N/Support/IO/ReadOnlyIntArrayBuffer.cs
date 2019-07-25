using System.Diagnostics.CodeAnalysis;

namespace ICU4N.Support.IO
{
    /// <summary>
    /// <see cref="Int32ArrayBuffer"/>, <see cref="ReadWriteInt32ArrayBuffer"/> and <see cref="ReadOnlyInt32ArrayBuffer"/> compose
    /// the implementation of array based <see cref="int"/> buffers.
    /// <para/>
    /// <see cref="ReadOnlyInt32ArrayBuffer"/> extends <see cref="Int32ArrayBuffer"/> with all the write methods
    /// throwing read only exception.
    /// <para/>
    /// This class is marked sealed for runtime performance.
    /// </summary>
    internal sealed class ReadOnlyInt32ArrayBuffer : Int32ArrayBuffer
    {
        internal static ReadOnlyInt32ArrayBuffer Copy(Int32ArrayBuffer other, int markOfOther)
        {
            ReadOnlyInt32ArrayBuffer buf = new ReadOnlyInt32ArrayBuffer(other
                    .Capacity, other.backingArray, other.offset);
            buf.limit = other.Limit;
            buf.position = other.Position;
            buf.mark = markOfOther;
            return buf;
        }

        internal ReadOnlyInt32ArrayBuffer(int capacity, int[] backingArray, int arrayOffset)
            : base(capacity, backingArray, arrayOffset)
        {
        }

        public override Int32Buffer AsReadOnlyBuffer()
        {
            return Duplicate();
        }

        public override Int32Buffer Compact()
        {
            throw new ReadOnlyBufferException();
        }

        public override Int32Buffer Duplicate()
        {
            return Copy(this, mark);
        }

        public override bool IsReadOnly
        {
            get { return true; }
        }

        [SuppressMessage("Microsoft.Performance", "CA1819", Justification = "design requires some writable array properties")]
        protected override int[] ProtectedArray
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

        public override Int32Buffer Put(int c)
        {
            throw new ReadOnlyBufferException();
        }

        public override Int32Buffer Put(int index, int c)
        {
            throw new ReadOnlyBufferException();
        }

        public override Int32Buffer Put(Int32Buffer buf)
        {
            throw new ReadOnlyBufferException();
        }

        public override sealed Int32Buffer Put(int[] src, int off, int len)
        {
            throw new ReadOnlyBufferException();
        }

        public override Int32Buffer Slice()
        {
            return new ReadOnlyInt32ArrayBuffer(Remaining, backingArray, offset
                    + position);
        }
    }
}
