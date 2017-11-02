using System;
using System.Diagnostics.CodeAnalysis;

namespace ICU4N.Support.IO
{
    /// <summary>
    /// CharArrayBuffer, ReadWriteCharArrayBuffer and ReadOnlyCharArrayBuffer compose
    /// the implementation of array based char buffers.
    /// <para/>
    /// ReadOnlyCharArrayBuffer extends CharArrayBuffer with all the write methods
    /// throwing read only exception.
    /// <para/>
    /// This class is marked sealed for runtime performance.
    /// </summary>
    internal sealed class ReadOnlyCharArrayBuffer : CharArrayBuffer
    {
        internal static ReadOnlyCharArrayBuffer Copy(CharArrayBuffer other, int markOfOther)
        {
            ReadOnlyCharArrayBuffer buf = new ReadOnlyCharArrayBuffer(other
                    .Capacity, other.backingArray, other.offset);
            buf.limit = other.Limit;
            buf.position = other.Position;
            buf.mark = markOfOther;
            return buf;
        }

        internal ReadOnlyCharArrayBuffer(int capacity, char[] backingArray, int arrayOffset)
            : base(capacity, backingArray, arrayOffset)
        {
        }

        public override CharBuffer AsReadOnlyBuffer()
        {
            return Duplicate();
        }

        public override CharBuffer Compact()
        {
            throw new ReadOnlyBufferException();
        }

        public override CharBuffer Duplicate()
        {
            return Copy(this, mark);
        }

        public override bool IsReadOnly
        {
            get { return true; }
        }

        [SuppressMessage("Microsoft.Performance", "CA1819", Justification = "design requires some writable array properties")]
        protected override char[] ProtectedArray
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

        public override CharBuffer Put(char c)
        {
            throw new ReadOnlyBufferException();
        }

        public override CharBuffer Put(int index, char c)
        {
            throw new ReadOnlyBufferException();
        }

        public override sealed CharBuffer Put(char[] src, int off, int len)
        {
            throw new ReadOnlyBufferException();
        }

        public override sealed CharBuffer Put(CharBuffer src)
        {
            throw new ReadOnlyBufferException();
        }

        public override CharBuffer Put(string src, int start, int end)
        {
            if ((start < 0) || (end < 0)
                    || (long)start + (long)end > src.Length)
            {
                throw new IndexOutOfRangeException();
            }
            throw new ReadOnlyBufferException();
        }

        public override CharBuffer Slice()
        {
            return new ReadOnlyCharArrayBuffer(Remaining, backingArray, offset
                    + position);
        }
    }
}
