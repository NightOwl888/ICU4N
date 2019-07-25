using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace ICU4N.Support.IO
{
    /// <summary>
    /// A buffer of shorts.
    /// <para/>
    /// A short buffer can be created in either of the following ways:
    /// <list type="bullet">
    ///     <item><description><see cref="Allocate(int)"/> a new long array and create a buffer
    ///     based on it</description></item>
    ///     <item><description><see cref="Wrap(short[])"/> an existing long array to create a new
    ///     buffer</description></item>
    ///     <item><description>Use <see cref="ByteBuffer.AsInt16Buffer()"/> to create a short 
    ///     buffer based on a byte buffer.</description></item>
    /// </list>
    /// </summary>
    public abstract class Int16Buffer : Buffer, IComparable<Int16Buffer>
    {
        /**
         * Creates a short buffer based on a newly allocated short array.
         * 
         * @param capacity
         *            the capacity of the new buffer.
         * @return the created short buffer.
         * @throws IllegalArgumentException
         *             if {@code capacity} is less than zero.
         */
        public static Int16Buffer Allocate(int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentException();
            }
            return new ReadWriteInt16ArrayBuffer(capacity);
        }

        /**
         * Creates a new short buffer by wrapping the given short array.
         * <p>
         * Calling this method has the same effect as
         * {@code wrap(array, 0, array.length)}.
         *
         * @param array
         *            the short array which the new buffer will be based on.
         * @return the created short buffer.
         */
        public static Int16Buffer Wrap(short[] array)
        {
            return Wrap(array, 0, array.Length);
        }

        /**
         * Creates a new short buffer by wrapping the given short array.
         * <p>
         * The new buffer's position will be {@code start}, limit will be
         * {@code start + len}, capacity will be the length of the array.
         *
         * @param array
         *            the short array which the new buffer will be based on.
         * @param start
         *            the start index, must not be negative and not greater than
         *            {@code array.length}.
         * @param len
         *            the length, must not be negative and not greater than
         *            {@code array.length - start}.
         * @return the created short buffer.
         * @exception IndexOutOfBoundsException
         *                if either {@code start} or {@code len} is invalid.
         */
        public static Int16Buffer Wrap(short[] array, int start, int len)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }
            if (start < 0 || len < 0 || (long)start + (long)len > array.Length)
            {
                throw new IndexOutOfRangeException();
            }

            Int16Buffer buf = new ReadWriteInt16ArrayBuffer(array);
            buf.position = start;
            buf.limit = start + len;

            return buf;
        }

        /**
         * Constructs a {@code ShortBuffer} with given capacity.
         *
         * @param capacity
         *            The capacity of the buffer
         */
        internal Int16Buffer(int capacity)
            : base(capacity)
        {
        }

        /**
         * Returns the short array which this buffer is based on, if there is one.
         * 
         * @return the short array which this buffer is based on.
         * @exception ReadOnlyBufferException
         *                if this buffer is based on an array, but it is read-only.
         * @exception UnsupportedOperationException
         *                if this buffer is not based on an array.
         */
        [SuppressMessage("Microsoft.Performance", "CA1819", Justification = "design requires some writable array properties")]
        public short[] Array
        {
            get { return ProtectedArray; }
        }

        /**
         * Returns the offset of the short array which this buffer is based on, if
         * there is one.
         * <p>
         * The offset is the index of the array corresponding to the zero position
         * of the buffer.
         *
         * @return the offset of the short array which this buffer is based on.
         * @exception ReadOnlyBufferException
         *                if this buffer is based on an array, but it is read-only.
         * @exception UnsupportedOperationException
         *                if this buffer is not based on an array.
         */
        public int ArrayOffset
        {
            get { return ProtectedArrayOffset; }
        }

        /**
         * Returns a read-only buffer that shares its content with this buffer.
         * <p>
         * The returned buffer is guaranteed to be a new instance, even if this
         * buffer is read-only itself. The new buffer's position, limit, capacity
         * and mark are the same as this buffer's.
         * <p>
         * The new buffer shares its content with this buffer, which means this
         * buffer's change of content will be visible to the new buffer. The two
         * buffer's position, limit and mark are independent.
         *
         * @return a read-only version of this buffer.
         */
        public abstract Int16Buffer AsReadOnlyBuffer();

        /**
         * Compacts this short buffer.
         * <p>
         * The remaining shorts will be moved to the head of the buffer, starting
         * from position zero. Then the position is set to {@code remaining()}; the
         * limit is set to capacity; the mark is cleared.
         *
         * @return this buffer.
         * @exception ReadOnlyBufferException
         *                if no changes may be made to the contents of this buffer.
         */
        public abstract Int16Buffer Compact();

        /**
         * Compare the remaining shorts of this buffer to another short buffer's
         * remaining shorts.
         * 
         * @param otherBuffer
         *            another short buffer.
         * @return a negative value if this is less than {@code otherBuffer}; 0 if
         *         this equals to {@code otherBuffer}; a positive value if this is
         *         greater than {@code otherBuffer}.
         * @exception ClassCastException
         *                if {@code otherBuffer} is not a short buffer.
         */
        public virtual int CompareTo(Int16Buffer otherBuffer)
        {
            int compareRemaining = (Remaining < otherBuffer.Remaining) ? Remaining
                    : otherBuffer.Remaining;
            int thisPos = position;
            int otherPos = otherBuffer.position;
            short thisByte, otherByte;
            while (compareRemaining > 0)
            {
                thisByte = Get(thisPos);
                otherByte = otherBuffer.Get(otherPos);
                if (thisByte != otherByte)
                {
                    return thisByte < otherByte ? -1 : 1;
                }
                thisPos++;
                otherPos++;
                compareRemaining--;
            }
            return Remaining - otherBuffer.Remaining;
        }

        /**
         * Returns a duplicated buffer that shares its content with this buffer.
         * <p>
         * The duplicated buffer's position, limit, capacity and mark are the same
         * as this buffer. The duplicated buffer's read-only property and byte order
         * are the same as this buffer's.
         * <p>
         * The new buffer shares its content with this buffer, which means either
         * buffer's change of content will be visible to the other. The two buffer's
         * position, limit and mark are independent.
         *
         * @return a duplicated buffer that shares its content with this buffer.
         */
        public abstract Int16Buffer Duplicate();

        /**
         * Checks whether this short buffer is equal to another object.
         * <p>
         * If {@code other} is not a short buffer then {@code false} is returned.
         * Two short buffers are equal if and only if their remaining shorts are
         * exactly the same. Position, limit, capacity and mark are not considered.
         *
         * @param other
         *            the object to compare with this short buffer.
         * @return {@code true} if this short buffer is equal to {@code other},
         *         {@code false} otherwise.
         */
        public override bool Equals(object other)
        {
            if (!(other is Int16Buffer))
            {
                return false;
            }
            Int16Buffer otherBuffer = (Int16Buffer)other;

            if (Remaining != otherBuffer.Remaining)
            {
                return false;
            }

            int myPosition = position;
            int otherPosition = otherBuffer.position;
            bool equalSoFar = true;
            while (equalSoFar && (myPosition < limit))
            {
                equalSoFar = Get(myPosition++) == otherBuffer.Get(otherPosition++);
            }

            return equalSoFar;
        }

        /**
         * Returns the short at the current position and increases the position by
         * 1.
         * 
         * @return the short at the current position.
         * @exception BufferUnderflowException
         *                if the position is equal or greater than limit.
         */
        public abstract short Get();

        /**
         * Reads shorts from the current position into the specified short array and
         * increases the position by the number of shorts read.
         * <p>
         * Calling this method has the same effect as
         * {@code get(dest, 0, dest.length)}.
         *
         * @param dest
         *            the destination short array.
         * @return this buffer.
         * @exception BufferUnderflowException
         *                if {@code dest.length} is greater than {@code remaining()}.
         */
        public virtual Int16Buffer Get(short[] dest)
        {
            return Get(dest, 0, dest.Length);
        }

        /**
         * Reads shorts from the current position into the specified short array,
         * starting from the specified offset, and increases the position by the
         * number of shorts read.
         * 
         * @param dest
         *            the target short array.
         * @param off
         *            the offset of the short array, must not be negative and not
         *            greater than {@code dest.length}.
         * @param len
         *            the number of shorts to read, must be no less than zero and
         *            not greater than {@code dest.length - off}.
         * @return this buffer.
         * @exception IndexOutOfBoundsException
         *                if either {@code off} or {@code len} is invalid.
         * @exception BufferUnderflowException
         *                if {@code len} is greater than {@code remaining()}.
         */
        public virtual Int16Buffer Get(short[] dest, int off, int len)
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
            for (int i = off; i < off + len; i++)
            {
                dest[i] = Get();
            }
            return this;
        }

        /**
         * Returns the short at the specified index; the position is not changed.
         * 
         * @param index
         *            the index, must not be negative and less than limit.
         * @return a short at the specified index.
         * @exception IndexOutOfBoundsException
         *                if index is invalid.
         */
        public abstract short Get(int index);

        /**
         * Indicates whether this buffer is based on a short array and is
         * read/write.
         *
         * @return {@code true} if this buffer is based on a short array and
         *         provides read/write access, {@code false} otherwise.
         */
        public bool HasArray
        {
            get { return ProtectedHasArray; }
        }

        /**
         * Calculates this buffer's hash code from the remaining chars. The
         * position, limit, capacity and mark don't affect the hash code.
         *
         * @return the hash code calculated from the remaining shorts.
         */
        public override int GetHashCode()
        {
            int myPosition = position;
            int hash = 0;
            while (myPosition < limit)
            {
                hash = hash + Get(myPosition++);
            }
            return hash;
        }

        /**
         * Indicates whether this buffer is direct. A direct buffer will try its
         * best to take advantage of native memory APIs and it may not stay in the
         * Java heap, so it is not affected by garbage collection.
         * <p>
         * A short buffer is direct if it is based on a byte buffer and the byte
         * buffer is direct.
         *
         * @return {@code true} if this buffer is direct, {@code false} otherwise.
         */
        public abstract bool IsDirect { get; }

        /**
         * Returns the byte order used by this buffer when converting shorts from/to
         * bytes.
         * <p>
         * If this buffer is not based on a byte buffer, then always return the
         * platform's native byte order.
         *
         * @return the byte order used by this buffer when converting shorts from/to
         *         bytes.
         */
        public abstract ByteOrder Order { get; }

        /**
         * Child class implements this method to realize {@code array()}.
         *
         * @return see {@code array()}
         */
        [SuppressMessage("Microsoft.Performance", "CA1819", Justification = "design requires some writable array properties")]
        protected abstract short[] ProtectedArray { get; }

        /**
         * Child class implements this method to realize {@code arrayOffset()}.
         *
         * @return see {@code arrayOffset()}
         */
        protected abstract int ProtectedArrayOffset { get; }

        /**
         * Child class implements this method to realize {@code hasArray()}.
         *
         * @return see {@code hasArray()}
         */
        protected abstract bool ProtectedHasArray { get; }

        /**
         * Writes the given short to the current position and increases the position
         * by 1.
         * 
         * @param s
         *            the short to write.
         * @return this buffer.
         * @exception BufferOverflowException
         *                if position is equal or greater than limit.
         * @exception ReadOnlyBufferException
         *                if no changes may be made to the contents of this buffer.
         */
        public abstract Int16Buffer Put(short s);

        /**
         * Writes shorts from the given short array to the current position and
         * increases the position by the number of shorts written.
         * <p>
         * Calling this method has the same effect as
         * {@code put(src, 0, src.length)}.
         *
         * @param src
         *            the source short array.
         * @return this buffer.
         * @exception BufferOverflowException
         *                if {@code remaining()} is less than {@code src.length}.
         * @exception ReadOnlyBufferException
         *                if no changes may be made to the contents of this buffer.
         */
        public Int16Buffer Put(short[] src)
        {
            return Put(src, 0, src.Length);
        }

        /**
         * Writes shorts from the given short array, starting from the specified
         * offset, to the current position and increases the position by the number
         * of shorts written.
         * 
         * @param src
         *            the source short array.
         * @param off
         *            the offset of short array, must not be negative and not
         *            greater than {@code src.length}.
         * @param len
         *            the number of shorts to write, must be no less than zero and
         *            not greater than {@code src.length - off}.
         * @return this buffer.
         * @exception BufferOverflowException
         *                if {@code remaining()} is less than {@code len}.
         * @exception IndexOutOfBoundsException
         *                if either {@code off} or {@code len} is invalid.
         * @exception ReadOnlyBufferException
         *                if no changes may be made to the contents of this buffer.
         */
        public virtual Int16Buffer Put(short[] src, int off, int len)
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
            for (int i = off; i < off + len; i++)
            {
                Put(src[i]);
            }
            return this;
        }

        /**
         * Writes all the remaining shorts of the {@code src} short buffer to this
         * buffer's current position, and increases both buffers' position by the
         * number of shorts copied.
         * 
         * @param src
         *            the source short buffer.
         * @return this buffer.
         * @exception BufferOverflowException
         *                if {@code src.remaining()} is greater than this buffer's
         *                {@code remaining()}.
         * @exception IllegalArgumentException
         *                if {@code src} is this buffer.
         * @exception ReadOnlyBufferException
         *                if no changes may be made to the contents of this buffer.
         */
        public virtual Int16Buffer Put(Int16Buffer src)
        {
            if (src == this)
            {
                throw new ArgumentException();
            }
            if (src.Remaining > Remaining)
            {
                throw new BufferOverflowException();
            }
            short[] contents = new short[src.Remaining];
            src.Get(contents);
            Put(contents);
            return this;
        }

        /**
         * Writes a short to the specified index of this buffer; the position is not
         * changed.
         * 
         * @param index
         *            the index, must not be negative and less than the limit.
         * @param s
         *            the short to write.
         * @return this buffer.
         * @exception IndexOutOfBoundsException
         *                if index is invalid.
         * @exception ReadOnlyBufferException
         *                if no changes may be made to the contents of this buffer.
         */
        public abstract Int16Buffer Put(int index, short s);

        /**
         * Returns a sliced buffer that shares its content with this buffer.
         * <p>
         * The sliced buffer's capacity will be this buffer's {@code remaining()},
         * and its zero position will correspond to this buffer's current position.
         * The new buffer's position will be 0, limit will be its capacity, and its
         * mark is cleared. The new buffer's read-only property and byte order are
         * same as this buffer's.
         * <p>
         * The new buffer shares its content with this buffer, which means either
         * buffer's change of content will be visible to the other. The two buffer's
         * position, limit and mark are independent.
         * 
         * @return a sliced buffer that shares its content with this buffer.
         */
        public abstract Int16Buffer Slice();

        /**
         * Returns a string representing the state of this short buffer.
         * 
         * @return a string representing the state of this short buffer.
         */
        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();
            buf.Append(GetType().Name);
            buf.Append(", status: capacity="); //$NON-NLS-1$
            buf.Append(Capacity);
            buf.Append(" position="); //$NON-NLS-1$
            buf.Append(Position);
            buf.Append(" limit="); //$NON-NLS-1$
            buf.Append(Limit);
            return buf.ToString();
        }
    }
}
