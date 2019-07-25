using System;
using System.Diagnostics.CodeAnalysis;

namespace ICU4N.Support.IO
{
    /// <summary>
    /// This class wraps a byte buffer to be a int buffer.
    /// </summary>
    /// <remarks>
    /// Implementation notice:
    /// <list type="bullet">
    ///     <item><description>
    ///         After a byte buffer instance is wrapped, it becomes privately owned by
    ///         the adapter. It must NOT be accessed outside the adapter any more.
    ///     </description></item>
    ///     <item><description>
    ///         The byte buffer's position and limit are NOT linked with the adapter.
    ///         The adapter extends <see cref="Buffer"/>, thus has its own position and limit.
    ///     </description></item>
    /// </list>
    /// </remarks>
    internal sealed class Int32ToByteBufferAdapter : Int32Buffer
    {
        internal static Int32Buffer Wrap(ByteBuffer byteBuffer)
        {
            return new Int32ToByteBufferAdapter(byteBuffer.Slice());
        }

        private readonly ByteBuffer byteBuffer;

        internal Int32ToByteBufferAdapter(ByteBuffer byteBuffer)
                : base((byteBuffer.Capacity >> 2))
        {
            this.byteBuffer = byteBuffer;
            this.byteBuffer.Clear();
        }

        //public int GetByteCapacity()
        //{
        //    if (byteBuffer is IDirectBuffer)
        //    {
        //        return ((IDirectBuffer)byteBuffer).GetByteCapacity();
        //    }
        //    Debug.Assert(false, byteBuffer);
        //    return -1;
        //}

        //public PlatformAddress getEffectiveAddress()
        //{
        //    if (byteBuffer instanceof DirectBuffer) {
        //        return ((DirectBuffer)byteBuffer).getEffectiveAddress();
        //    }
        //    assert false : byteBuffer;
        //    return null;
        //}

        //public PlatformAddress getBaseAddress()
        //{
        //    if (byteBuffer instanceof DirectBuffer) {
        //        return ((DirectBuffer)byteBuffer).getBaseAddress();
        //    }
        //    assert false : byteBuffer;
        //    return null;
        //}

        //public boolean isAddressValid()
        //{
        //    if (byteBuffer instanceof DirectBuffer) {
        //        return ((DirectBuffer)byteBuffer).isAddressValid();
        //    }
        //    assert false : byteBuffer;
        //    return false;
        //}

        //public void addressValidityCheck()
        //{
        //    if (byteBuffer instanceof DirectBuffer) {
        //        ((DirectBuffer)byteBuffer).addressValidityCheck();
        //    } else {
        //        assert false : byteBuffer;
        //    }
        //}

        //public void free()
        //{
        //    if (byteBuffer instanceof DirectBuffer) {
        //        ((DirectBuffer)byteBuffer).free();
        //    } else {
        //        assert false : byteBuffer;
        //    }
        //}

        public override Int32Buffer AsReadOnlyBuffer()
        {
            Int32ToByteBufferAdapter buf = new Int32ToByteBufferAdapter(byteBuffer
                    .AsReadOnlyBuffer());
            buf.limit = limit;
            buf.position = position;
            buf.mark = mark;
            return buf;
        }

        public override Int32Buffer Compact()
        {
            if (byteBuffer.IsReadOnly)
            {
                throw new ReadOnlyBufferException();
            }
            byteBuffer.Limit = limit << 2;
            byteBuffer.Position = position << 2;
            byteBuffer.Compact();
            byteBuffer.Clear();
            position = limit - position;
            limit = capacity;
            mark = UNSET_MARK;
            return this;
        }

        public override Int32Buffer Duplicate()
        {
            Int32ToByteBufferAdapter buf = new Int32ToByteBufferAdapter(byteBuffer
                    .Duplicate());
            buf.limit = limit;
            buf.position = position;
            buf.mark = mark;
            return buf;
        }

        public override int Get()
        {
            if (position == limit)
            {
                throw new BufferUnderflowException();
            }
            return byteBuffer.GetInt32(position++ << 2);
        }

        public override int Get(int index)
        {
            if (index < 0 || index >= limit)
            {
                throw new IndexOutOfRangeException();
            }
            return byteBuffer.GetInt32(index << 2);
        }

        public override bool IsDirect
        {
            get { return byteBuffer.IsDirect; }
        }

        public override bool IsReadOnly
        {
            get { return byteBuffer.IsReadOnly; }
        }

        public override ByteOrder Order
        {
            get { return byteBuffer.Order; }
        }

        [SuppressMessage("Microsoft.Performance", "CA1819", Justification = "design requires some writable array properties")]
        protected override int[] ProtectedArray
        {
            get { throw new NotSupportedException(); }
        }

        protected override int ProtectedArrayOffset
        {
            get { throw new NotSupportedException(); }
        }

        protected override bool ProtectedHasArray
        {
            get { return false; }
        }

        public override Int32Buffer Put(int c)
        {
            if (position == limit)
            {
                throw new BufferOverflowException();
            }
            byteBuffer.PutInt32(position++ << 2, c);
            return this;
        }

        public override Int32Buffer Put(int index, int c)
        {
            if (index < 0 || index >= limit)
            {
                throw new IndexOutOfRangeException();
            }
            byteBuffer.PutInt32(index << 2, c);
            return this;
        }

        public override Int32Buffer Slice()
        {
            byteBuffer.Limit = limit << 2;
            byteBuffer.Position = position << 2;
            Int32Buffer result = new Int32ToByteBufferAdapter(byteBuffer.Slice());
            byteBuffer.Clear();
            return result;
        }
    }
}
