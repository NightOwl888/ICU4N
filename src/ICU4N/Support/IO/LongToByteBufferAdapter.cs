﻿using System;
using System.Diagnostics.CodeAnalysis;

namespace ICU4N.Support.IO
{
    /// <summary>
    /// This class wraps a byte buffer to be a long buffer.
    /// <para/>
    /// Implementation notice:
    /// <list type="bullet">
    ///     <item><description>After a byte buffer instance is wrapped, it becomes privately owned by
    ///     the adapter. It must NOT be accessed outside the adapter any more.</description></item>
    ///     <item><description>The byte buffer's position and limit are NOT linked with the adapter.
    ///     The adapter extends Buffer, thus has its own position and limit.</description></item>
    /// </list>
    /// </summary>
    internal sealed class Int64ToByteBufferAdapter : Int64Buffer
    {
        internal static Int64Buffer Wrap(ByteBuffer byteBuffer)
        {
            return new Int64ToByteBufferAdapter(byteBuffer.Slice());
        }

        private readonly ByteBuffer byteBuffer;

        internal Int64ToByteBufferAdapter(ByteBuffer byteBuffer)
            : base((byteBuffer.Capacity >> 3))
        {
            this.byteBuffer = byteBuffer;
            this.byteBuffer.Clear();
        }

        //public int GetByteCapacity()
        //{
        //    if (byteBuffer is DirectBuffer) {
        //        return ((DirectBuffer)byteBuffer).GetByteCapacity();
        //    }
        //    return -1;
        //}

        //public PlatformAddress getEffectiveAddress()
        //{
        //    if (byteBuffer is DirectBuffer) {
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

        public override Int64Buffer AsReadOnlyBuffer()
        {
            throw new NotImplementedException();
            //Int64ToByteBufferAdapter buf = new Int64ToByteBufferAdapter(byteBuffer
            //        .AsReadOnlyBuffer());
            //buf.limit = limit;
            //buf.position = position;
            //buf.mark = mark;
            //return buf;
        }

        public override Int64Buffer Compact()
        {
            if (byteBuffer.IsReadOnly)
            {
                throw new ReadOnlyBufferException();
            }
            byteBuffer.SetLimit(limit << 3);
            byteBuffer.SetPosition(position << 3);
            byteBuffer.Compact();
            byteBuffer.Clear();
            position = limit - position;
            limit = capacity;
            mark = UnsetMark;
            return this;
        }


        public override Int64Buffer Duplicate()
        {
            Int64ToByteBufferAdapter buf = new Int64ToByteBufferAdapter(byteBuffer
                    .Duplicate());
            buf.limit = limit;
            buf.position = position;
            buf.mark = mark;
            return buf;
        }


        public override long Get()
        {
            if (position == limit)
            {
                throw new BufferUnderflowException();
            }
            return byteBuffer.GetInt64(position++ << 3);
        }


        public override long Get(int index)
        {
            if (index < 0 || index >= limit)
            {
                throw new IndexOutOfRangeException();
            }
            return byteBuffer.GetInt64(index << 3);
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


        //[WritableArray]
        [SuppressMessage("Microsoft.Performance", "CA1819", Justification = "design requires some writable array properties")]
        protected override long[] ProtectedArray
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


        public override Int64Buffer Put(long c)
        {
            if (position == limit)
            {
                throw new BufferOverflowException();
            }
            byteBuffer.PutInt64(position++ << 3, c);
            return this;
        }


        public override Int64Buffer Put(int index, long c)
        {
            if (index < 0 || index >= limit)
            {
                throw new IndexOutOfRangeException();
            }
            byteBuffer.PutInt64(index << 3, c);
            return this;
        }


        public override Int64Buffer Slice()
        {
            byteBuffer.SetLimit(limit << 3);
            byteBuffer.SetPosition(position << 3);
            Int64Buffer result = new Int64ToByteBufferAdapter(byteBuffer.Slice());
            byteBuffer.Clear();
            return result;
        }
    }
}
